using Godot;
using System;

public partial class Marceline : NPC
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		portraitPath = "MarcelinePortrait.png";

		trueName = "Marceline";
		Globals.AddNPC(this);

		diagPath = "d1.txt";
		LoadDialogue(diagPath);

		voicePath = "Marceline.wav";
		SetVoice(voicePath);

		missionJSONPath = "Olivia.json";
		SetMissionsJSON(missionJSONPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
