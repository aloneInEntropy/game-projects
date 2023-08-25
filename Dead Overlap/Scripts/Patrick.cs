using Godot;
using System;

public partial class Patrick : NPC
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		portraitPath = "PatrickPortrait.png";

		trueName = "Patrick";
		Globals.AddNPC(this);

		// diagPath = "d1.txt";
		LoadDialogue(diagPath);

		voicePath = "Mark.wav";
		SetVoice(voicePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
