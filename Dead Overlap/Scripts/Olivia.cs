using Godot;
using System;

public partial class Olivia : NPC
{
	public override void _Ready() {
		trueName = "Olivia";
		Globals.AddNPC(this);

		diagPath = "res://Dialogue/d1.txt";
		LoadDialogue(diagPath);

		voicePath = "res://Assets/Audio/Voices/Olivia.wav";
		SetVoice(voicePath);

		missionJSONPath = "res://Scripts/Missions/Olivia.json";
		SetMissionsJSON(missionJSONPath);
	}

	public override void _Process(double delta) {
		// if (diagPath != "res://Dialogue/d1.txt") diagPath = "res://Dialogue/d1.txt";
	}

	public void SetMissionActiveState(int n, bool b) {

	}
}
