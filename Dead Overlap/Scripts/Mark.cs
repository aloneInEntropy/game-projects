using Godot;
using System;

public partial class Mark : NPC
{	
	public override void _Ready() {
		portraitPath = "MakotoPortrait.png";

		trueName = "Mark";
		Globals.AddNPC(this);

		diagPath = "d3.txt";
		LoadDialogue(diagPath);

		voicePath = "Mark.wav";
		SetVoice(voicePath);

		missionJSONPath = "Mark.json";
		SetMissionsJSON(missionJSONPath);
	}

	public override void _Process(double delta) {
	}
}
