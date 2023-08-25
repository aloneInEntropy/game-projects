using Godot;


/// <summary>
/// The Audio Manager. Handles all audio events.
/// </summary>
public partial class AudioManager : Node
{
	public static AudioStreamPlayer voicePlayer = new();
	public static AudioStreamPlayer footstepPlayer = new();
	public static AudioStreamPlayer oneShotPlayer = new();

	public static Timer stepTimer = new();

	/// <summary>
	/// How frames to pass before playing the dialogue "voice" of a particular NPC or description.
	/// </summary>
	public static float updateVoiceSpeed = 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		voicePlayer = GetNode<AudioStreamPlayer>("VoicePlayer");
		footstepPlayer = GetNode<AudioStreamPlayer>("StepAudio");
		oneShotPlayer = GetNode<AudioStreamPlayer>("OneShotOther");
		stepTimer = GetNode<Timer>("StepTimer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Play an audio clip once using the path to the audio source.
	/// </summary>
	/// <param name="pathTo"></param>
	public static void PlayOnce(string pathTo) {
		oneShotPlayer.Stream = GD.Load<AudioStream>(pathTo);
		oneShotPlayer.Play();
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
