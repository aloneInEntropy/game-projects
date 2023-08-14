using Godot;
using System;

public partial class Mark : NPC
{	
	public override void _Ready() {
		diagPath = "res://Dialogue/d3.txt";
		// diagPath = "res://Dialogue/d2.txt";
		LoadDialogue(diagPath);
		voicePath = "res://Assets/Audio/Voices/Mark.wav";
		voice = (AudioStreamWav)GD.Load(voicePath);
	}

	public override void _Process(double delta) {
		// if (diagPath != "res://Dialogue/d3.txt") diagPath = "res://Dialogue/d3.txt";
		// if (diagPath != "res://Dialogue/d2.txt") diagPath = "res://Dialogue/d2.txt";
	}
}
