using Godot;
using System;

public partial class Johnny : NPC
{
	public override void _Ready()
	{
		portraitPath = "JohnnyPortrait.png";

		trueName = "Johnny";
		Globals.AddNPC(this);

		diagPath = "d3.txt";
		LoadDialogue(diagPath);

		voicePath = "Mark.wav";
		SetVoice(voicePath);

		missionJSONPath = "Mark.json";
		SetMissionsJSON(missionJSONPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
