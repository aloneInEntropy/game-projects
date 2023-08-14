using Godot;
using System;

public partial class AudioManager : Node
{
	public static AudioStreamPlayer voicePlayer = new();

	/// <summary>
	/// How frames to pass before playing the dialogue "voice" of a particular NPC or description.
	/// </summary>
	public static float updateVoiceSpeed = 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		voicePlayer = GetNode<AudioStreamPlayer>("VoicePlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public static void PlayVoice(AudioStream audioStream) {
		voicePlayer.Stream = audioStream;
		voicePlayer.Play();
	}

	public static void SetUpdateVoiceSpeed(float n) {
		updateVoiceSpeed = n;
	}
}
