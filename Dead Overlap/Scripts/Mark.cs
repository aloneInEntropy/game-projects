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
	}

	public override void _Process(double delta) {
	}
}
