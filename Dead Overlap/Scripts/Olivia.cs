using Godot;
using System;

public partial class Olivia : NPC
{
	public override void _Ready() {
		portraitPath = "AdoraPortrait.png";

		trueName = "Olivia";
		Globals.AddNPC(this);

		diagPath = "d1.txt";
		LoadDialogue(diagPath);

		voicePath = "Olivia.wav";
		SetVoice(voicePath);

		missionJSONPath = "Olivia.json";
		SetMissionsJSON(missionJSONPath);
	}

	public override void _Process(double delta) {
		
	}
}
