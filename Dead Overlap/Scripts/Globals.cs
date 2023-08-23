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
	/// The current scene.
	/// </summary>
	public static PackedScene currentScene = new();

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
	/// The global HashSet of NPCs.
	/// </summary>
	public static HashSet<NPC> nPCs = new();

	/// <summary>
	/// The current day the Player is exploring in.
	/// </summary>
	public static int day = new();

	/// <summary>
    /// Static JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions options = new() { IncludeFields = true, WriteIndented = true };

	/// <summary>
	/// The path to the notes.json file in standard format.
	/// </summary>
	public static string pathToNotes = "Assets/Text/Notes/notes.json";

	/// <summary>
	/// The path to the dialogue file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToDialogue = "res://Assets/Text/Dialogue/";
	
	/// <summary>
	/// The path to the missions file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToMissions = "res://Assets/Text/Missions/";
	
	/// <summary>
	/// The path to the missions file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToVoice = "res://Assets/Audio/Voices/";
	
	/// <summary>
	/// The path to the missions file folder in Godot format (prepended with "res://")
	/// </summary>
	public static string resPathToPortraits = "res://Assets/Sprites/Portraits/";

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
		nPCs.Add(n);
	}
	
	/// <summary>
	/// Get the NPC of name <c>n</c> if it exists, and return null otherwise.
	/// </summary>
	/// <param name="n"></param>
	/// <returns></returns>
	public static NPC GetNPC(string n) {
		return nPCs.FirstOrDefault(m => m.trueName == n);
	}
}

