using Godot;
using System.Linq;
using System.Collections.Generic;


/// <summary>
/// The Globals class. Contains references to common global objects for quick convenience.
/// </summary>
public partial class Globals : Node
{
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
