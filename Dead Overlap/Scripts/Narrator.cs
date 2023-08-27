using Godot;
using System;

/// <summary>
/// The Narrator NPC. Contains the default voice path.
/// </summary>
public partial class Narrator : NPC
{
	public override void _Ready() {
		portraitPath = "NarratorPortrait.png";

		trueName = "Narrator";
		Globals.AddNPC(this);

		voicePath = "Mark.wav";
		SetVoice(voicePath);
	}
}
