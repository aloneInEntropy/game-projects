using Godot;
using System;

public partial class Adora : NPC
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		portraitPath = "AdoraPortrait.png";

		trueName = "Adora";
		Globals.AddNPC(this);

		diagPath = "d4.txt";
		LoadDialogue(diagPath);

		voicePath = "Adora.wav";
		SetVoice(voicePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
