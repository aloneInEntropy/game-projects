using Godot;
using System;

public partial class Olivia : NPC
{
	public override void _Ready() {
		diagPath = "res://Dialogue/d1.txt";
		LoadDialogue(diagPath);
		voicePath = "res://Assets/Audio/Voices/Olivia.wav";
		voice = (AudioStreamWav)GD.Load(voicePath);
	}

	public override void _Process(double delta) {
		// if (diagPath != "res://Dialogue/d1.txt") diagPath = "res://Dialogue/d1.txt";
	}
}
