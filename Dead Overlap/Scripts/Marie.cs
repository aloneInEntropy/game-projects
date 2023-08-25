using Godot;
using System;

public partial class Marie : NPC
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		portraitPath = "MariePortrait.png";

		trueName = "Marie";
		Globals.AddNPC(this);

		// diagPath = "d1.txt";
		LoadDialogue(diagPath);

		voicePath = "Olivia.wav";
		SetVoice(voicePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
