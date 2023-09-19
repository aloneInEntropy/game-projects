using System.Collections.Generic;
using System.Text.Json;
using Godot;
using System.IO;

public partial class PlayerVariables : Node
{
	public static Dictionary<string, object> playerVals = new();
	public static Dictionary<string, bool> checks = new();
	public static Dictionary<string, int> playerInfo = new();
	public static Dictionary<string, string> locationInfo = new();

    /// <summary>
	/// The list of missions given to the Player by this NPC.
	/// </summary>
	public static List<Mission> Missions { set;  get; }
    
	/// <summary>
	/// The list of clues found by the Player.
	/// </summary>
	public static List<Clue> Clues { set;  get; }


    public override void _Ready() {}

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
	/// Load the Player variables from a JSON file <c>variablesPath</c>. Uses a Node <c>tether</c> to access the current SceneTree.
	/// </summary>
	public static void LoadPlayerVariables(Node tether, string variablesPath) {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + variablesPath, Godot.FileAccess.ModeFlags.Read);
		playerVals = JsonSerializer.Deserialize<Dictionary<string, object>>(file.GetAsText(), Globals.options);
		checks = JsonSerializer.Deserialize<Dictionary<string, bool>>(playerVals["checks"].ToString());
		playerInfo = JsonSerializer.Deserialize<Dictionary<string, int>>(playerVals["player"].ToString());
		locationInfo = JsonSerializer.Deserialize<Dictionary<string, string>>(playerVals["location"].ToString());
		GameManager.sceneChangePosition = new Vector2(	
			playerInfo["xpos"],
			playerInfo["ypos"]
		);
		GameManager.sceneChangeFacing = new Vector2(	
			playerInfo["xdir"],
			playerInfo["ydir"]
		);
		SceneManager.SetScene(tether, locationInfo["locationName"]);

		file.Close();
	}

    /// <summary>
	/// Save the Player variables from a JSON file <c>variablesPath</c>. Uses a Node <c>tether</c> to access the current SceneTree.
	/// </summary>
	public static void SavePlayerVariables(string variablesPath) {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + variablesPath, Godot.FileAccess.ModeFlags.Read);
		
		// Saving PlayerInfo
		playerInfo["xpos"] = (int)Globals.player.Position.X;
		playerInfo["ypos"] = (int)Globals.player.Position.Y;
		playerInfo["xdir"] = (int)Globals.player.lastDirection.X;
		playerInfo["ydir"] = (int)Globals.player.lastDirection.Y;

		// Saving LocationInfo
		locationInfo["locationName"] = Globals.currentLocation.Name;

		// Saving PlayerVariables (Checks)
		// Checks are saved automatically.

		playerVals["checks"] = checks;
		playerVals["player"] = playerInfo;
		playerVals["location"] = locationInfo;
		string d = JsonSerializer.Serialize(playerVals, Globals.options);
		File.WriteAllText("Data/variables.json", d);
		// GD.Print(d);
		file.Close();
	}

    /// <summary>
	/// Set the missions of the NPC using the <c>missionPath</c> to a JSON file.
	/// </summary>
	/// <param name="missionPath"></param>
	public static void LoadMissions(string missionPath) {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + missionPath, Godot.FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		Missions = JsonSerializer.Deserialize<List<Mission>>(jsonString, Globals.options);
		foreach (Mission m in Missions) m.MType.Init();
		file.Close();
	}
    
	/// <summary>
	/// Save the missions of the NPC using the <c>missionPath</c> to a JSON file.
	/// </summary>
	/// <param name="missionPath"></param>
	public static void SaveMissions(string missionPath) {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + missionPath, Godot.FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		var tmissions = JsonSerializer.Deserialize<List<Mission>>(jsonString, Globals.options);
		foreach (Mission m in tmissions) {
			m.Active = GetMission(m.Name).Active;
			m.Completed = GetMission(m.Name).Completed;
		}
		string d = JsonSerializer.Serialize(Missions, Globals.options);
		File.WriteAllText("Data/missions.json", d);
		// GD.Print(d);
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

	/// <summary>
	/// Set the clues found by the Player using the <c>cluesPath</c> to a JSON file.
	/// </summary>
	/// <param name="cluesPath"></param>
	public static void LoadClues(string cluePath) {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + cluePath, Godot.FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		Clues = JsonSerializer.Deserialize<List<Clue>>(jsonString, Globals.options);
		file.Close();
	}
    
	/// <summary>
	/// Save the clues the Player has found using the <c>cluePath</c> to a JSON file.
	/// </summary>
	/// <param name="missionPath"></param>
	public static void SaveClues(string cluePath) {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + cluePath, Godot.FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		var tclues = JsonSerializer.Deserialize<List<Clue>>(jsonString, Globals.options);
		foreach (Clue c in tclues) {
			c.Found = GetClueSH(c.Shorthand).Found;
		}
		string d = JsonSerializer.Serialize(Clues, Globals.options);
		File.WriteAllText("Data/clues.json", d);
		file.Close();
	}

	/// <summary>
	/// Get a clue given its name <c>n</c>.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>The clue with the title <c>n</c>.</returns>
	public static Clue GetClue(string n) {
		foreach (var m in Clues) {
			if (m.Title == n) return m;
		}
		return null;
	}
	
	/// <summary>
	/// Get a clue given its shorthand name <c>n</c>.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>The clue with the shorthand name <c>n</c>.</returns>
	public static Clue GetClueSH(string n) {
		foreach (var m in Clues) {
			if (m.Shorthand == n) return m;
		}
		return null;
	}
	
	/// <summary>
	/// Get all clues that have been found.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>All clues found by the Player.</returns>
	public static List<Clue> GetFoundClues() {
		return Clues.FindAll(m => m.Found);
	}
}