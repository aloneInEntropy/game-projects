using Godot;


/// <summary>
/// The Audio Manager. Handles all audio events.
/// </summary>
public partial class AudioManager : Node
{
	public static AudioStreamPlayer voicePlayer = new();
	public static AudioStreamPlayer footstepPlayer = new();

	private static Timer stepTimer = new();

	/// <summary>
	/// How frames to pass before playing the dialogue "voice" of a particular NPC or description.
	/// </summary>
	public static float updateVoiceSpeed = 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		voicePlayer = GetNode<AudioStreamPlayer>("VoicePlayer");
		footstepPlayer = GetNode<AudioStreamPlayer>("StepAudio");
		stepTimer = GetNode<Timer>("StepTimer");
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

	public static void PlayStep() {
		if (stepTimer.TimeLeft <= 0) {
			footstepPlayer.Play();
			stepTimer.Start();
		}
	}

	void OnTimerTimeout() {
	}
}
