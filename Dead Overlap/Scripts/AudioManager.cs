using Godot;


/// <summary>
/// The Audio Manager. Handles all audio events.
/// </summary>
public partial class AudioManager : Node
{
	public static AudioStreamPlayer voicePlayer = new();
	public static AudioStreamPlayer footstepPlayer = new();
	public static AudioStreamPlayer oneShotPlayer = new();
	public static AudioStreamPlayer bgMusicPlayer = new();

	public static Timer stepTimer = new();

	/// <summary>
	/// How frames to pass before playing the dialogue "voice" of a particular NPC or description.
	/// </summary>
	public static float updateVoiceSpeed = 4;

	/// <summary>
	/// The name of the global (master) audio bus.
	/// </summary>
	[Export]
	public static string globalAudioBusName = "Master";

	/// <summary>
	/// The index of the global (master) audio bus.
	/// </summary>
	public static int globalAudioBusIndex = AudioServer.GetBusIndex(globalAudioBusName);
	
	/// <summary>
	/// The name of the music audio bus.
	/// </summary>
	[Export]
	public static string musicAudioBusName = "Music";

	/// <summary>
	/// The index of the master audio bus.
	/// </summary>
	public static int musicAudioBusIndex = AudioServer.GetBusIndex(globalAudioBusName);

	/// <summary>
	/// The value of the master volume on a scale of 0-100.
	/// </summary>
	public static float masterVolume = 90f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		voicePlayer = GetNode<AudioStreamPlayer>("VoicePlayer");
		footstepPlayer = GetNode<AudioStreamPlayer>("StepAudio");
		oneShotPlayer = GetNode<AudioStreamPlayer>("OneShotOther");
		bgMusicPlayer = GetNode<AudioStreamPlayer>("BGMusic");
		stepTimer = GetNode<Timer>("StepTimer");
		masterVolume = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(globalAudioBusIndex));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Globals.currentLocation.Name == "Hub") {
			bgMusicPlayer.Stop();
		} else {
			if (!bgMusicPlayer.Playing) {
				bgMusicPlayer.Play();
			}
		}
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
