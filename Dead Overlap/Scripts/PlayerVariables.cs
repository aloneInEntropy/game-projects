using System.Collections.Generic;
using System.Text.Json;
using Godot;

public partial class PlayerVariables : Node
{
	public static Dictionary<string, object> playerVals = new();
	public static Dictionary<string, bool> checks = new();
	public static Dictionary<string, int> stuff = new();

    /// <summary>
	/// The list of missions given to the Player by this NPC.
	/// </summary>
	public static List<Mission> Missions { set;  get; }


    public override void _Ready() {
        SetMissionsJSON("missions.json");
		LoadPlayerVariables("variables.json");
    }

	/// <summary>
	/// Set a player variable <c>varName</c> to the value <c>val</c>. <c>val</c> will be determined depending on <c>varName</c>.
	/// </summary>
	/// <param name="varName"></param>
	/// <param name="val"></param>
	public static void SetCheck(string varName, bool val) {
		checks[varName] = val;
	}

	/// <summary>
	/// Get a player variable boolean <c>varName</c>.
	/// </summary>
	/// <param name="varName"></param>
	/// <param name="val"></param>
	public static bool GetCheck(string varName) {
		return checks[varName];
    }

    /// <summary>
	/// Load the Player variables from a JSON file <c>variablesPath</c>.
	/// </summary>
	/// <param name="missionPath"></param>
	public void LoadPlayerVariables(string variablesPath) {
		FileAccess file = FileAccess.Open(Globals.resPathToData + variablesPath, FileAccess.ModeFlags.Read);
		playerVals = JsonSerializer.Deserialize<Dictionary<string, object>>(file.GetAsText(), Globals.options);
		checks = JsonSerializer.Deserialize<Dictionary<string, bool>>(playerVals["checks"].ToString());
		stuff = JsonSerializer.Deserialize<Dictionary<string, int>>(playerVals["stuff"].ToString());
		file.Close();
	}

    /// <summary>
	/// Set the missions of the NPC using the <c>missionPath</c> to a JSON file.
	/// </summary>
	/// <param name="missionPath"></param>
	public void SetMissionsJSON(string missionPath) {
		FileAccess file = FileAccess.Open(Globals.resPathToData + missionPath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		Missions = JsonSerializer.Deserialize<List<Mission>>(jsonString, Globals.options);
		foreach (Mission m in Missions) m.MType.Init();
		file.Close();
	}

	/// <summary>
	/// Get a mission given its name <c>n</c>.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>The mission with the name <c>n</c>.</returns>
	public static Mission GetMission(string n) {
		foreach (var m in Missions) {
			if (m.Name == n) return m;
		}
		return null;
	}

    public static void printMissions() {
        foreach (var m in GetUncompletedMissions()) {
            GD.Print(m.Name);
        }
    }
	
	/// <summary>
	/// Get a mission at position <c>n</c> in the stored list of missions.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>The mission at position <c>n</c>.</returns>
	public static Mission GetMission(int n) {
		return Missions[n];
	}
	
	/// <summary>
	/// Get all missions not yet completed.
	/// </summary>
	/// <returns>The list of missions that aren't completed.</returns>
	public static List<Mission> GetUncompletedMissions() {
		return Missions.FindAll(m => !m.Completed);
	}

	/// <summary>
	/// Get all missions currently tracking completion.
	/// </summary>
	/// <returns>The list of missions tracking completion.</returns>
	public static List<Mission> GetActiveMissions() {
		return Missions.FindAll(m => m.Active);
	}
	
    /// <summary>
	/// Get all missions active but uncompleted missions.
	/// </summary>
	/// <returns>The list of missions to next be completed.</returns>
	public static List<Mission> GetNextMissions() {
		return Missions.FindAll(m => !m.Completed && m.Active);
	}
}