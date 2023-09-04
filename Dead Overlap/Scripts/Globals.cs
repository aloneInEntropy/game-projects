using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System;


/// <summary>
/// The Globals class. Contains references to common global objects for quick convenience.
/// </summary>
public partial class Globals : Node
{
	/// <summary>
	/// The current Location the Player is in.
	/// </summary>
	public static Location currentLocation = new();

	/// <summary>
	/// The global onscreen GUI.
	/// </summary>
	public static GUI gui = new();

	/// <summary>
	/// The global Player character.
	/// </summary>
	public static Player player = new();

	/// <summary>
	/// The presently-talking NPC.
	/// </summary>
	public static NPC talkingNPC = new();
	
	/// <summary>
	/// The name of the presently-talking NPC. Not necessarily the actual talking NPC.
	/// </summary>
	public static string talkingNPCName;

	/// <summary>
	/// The global HashSet of NPCs.
	/// </summary>
	public static HashSet<NPC> nPCs = new();

	/// <summary>
	/// The current day the Player is exploring in.
	/// </summary>
	public static int day = 0;

	/// <summary>
    /// Static JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions options = new() { IncludeFields = true, WriteIndented = true };

	/// <summary>
	/// The path to the player Data folder in standard format.
	/// </summary>
	public static string pathToData = "Data/";

	/// <summary>
	/// The path to the player Data folder in Godot format (prepended with "res://)
	/// </summary>
	public static string resPathToData = "res://Data/";

	/// <summary>
	/// The path to the notes.json file in standard format.
	/// </summary>
	public static string pathToNotes = "Data/notes.json";
	
	/// <summary>
	/// The path to the missions.json file in standard format.
	/// </summary>
	public static string pathToMissions = "Data/missions.json";

	/// <summary>
	/// The path to the dialogue file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToDialogue = "res://Assets/Text/Dialogue/";
	
	/// <summary>
	/// The path to the missions file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToMissions = "res://Data/";
	
	/// <summary>
	/// The path to the voice file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToVoice = "res://Assets/Audio/Voices/";
	
	/// <summary>
	/// The path to the portrait file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToPortraits = "res://Assets/Sprites/Portraits/";
	
	/// <summary>
	/// The path to the showcase file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToShowcases = "res://Assets/Sprites/Showcases/";

	/// <summary>
	/// The timeline of days the events take place. Use dayToDate[day] to get the string date of that day.
	/// </summary>
	public static string[] dayToDate = new[] {
		"September 7th",
		"September 8th",
		"September 9th",
		"September 10th",
		"September 11th",
		"September 12th"
	};


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        day = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Add an NPC <c>n</c> to the global set of NPCs.
	/// </summary>
	/// <param name="n"></param>
	public static void AddNPC(NPC n) {
		_ = GetNPC(n.trueName) is null ? nPCs.Add(n) : UpdateNPCExt(n);
		/* if (GetNPC(n.trueName) is null) {
			nPCs.Add(n);
		} else {
			UpdateNPCExt(n);
		} */
	}
	
	/// <summary>
	/// Get the NPC of name <c>n</c> if it exists, and return null otherwise.
	/// </summary>
	/// <param name="n"></param>
	/// <returns></returns>
	public static NPC GetNPC(string n) {
		return nPCs.FirstOrDefault(m => m.trueName == n);
	}

	/// <summary>
	/// Update the NPC in the internal Global list of NPCs. Returns <c>true</c> if the internal NPC was updated.
	/// </summary>
	/// <param name="n"></param>
	public static bool UpdateNPCInt(NPC externalNPC) {
		var m = GetNPC(externalNPC.trueName);
		if (m is not null) {
			m.diagPath = externalNPC.diagPath;
			return true;
		}
		return false;
	}
	
	/// <summary>
	/// Update the given NPC by applying the values from the NPC in the internal Global list of NPCs. Returns <c>true</c> if the external NPC was updated.
	/// </summary>
	/// <param name="n"></param>
	public static bool UpdateNPCExt(NPC externalNPC) {
		var m = GetNPC(externalNPC.trueName);
		if (m is not null) {
			externalNPC.diagPath = m.diagPath;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Load a scene asyncronously. This uses the FadeOut animation in the current Location to work. <br/>
	/// If you need to load a scene statically, use ChangeScene.
	/// </summary>
	/// <param name="sceneName"></param>
	public async void LoadScene(string sceneName) {
		GameManager.isGamePaused = false;
		GameManager.canPauseGame = false;
		currentLocation.Leave();
		await ToSignal(currentLocation.animationPlayer, "animation_finished");
		currentLocation.GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
	}

	/// <summary>
	/// Load a scene <c>sceneName</c>.
	/// </summary>
	/// <param name="sceneName"></param>
	public static void ChangeScene(string sceneName) {
		currentLocation.GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
	}
}

