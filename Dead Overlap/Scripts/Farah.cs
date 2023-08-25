using Godot;
using System;

public partial class Farah : NPC
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		portraitPath = "FarahPortrait.png";

		trueName = "Farah";
		Globals.AddNPC(this);

		// diagPath = "d4.txt";
		LoadDialogue(diagPath);

		voicePath = "Olivia.wav";
		SetVoice(voicePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
