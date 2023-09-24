using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;


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
	public static string MusicAudioBusName = "Music";

	/// <summary>
	/// The index of the music audio bus.
	/// </summary>
	public static int MusicAudioBusIndex = AudioServer.GetBusIndex(MusicAudioBusName);
	
	/// <summary>
	/// The name of the SFX audio bus.
	/// </summary>
	[Export]
	public static string SFXAudioBusName = "SFX";

	/// <summary>
	/// The index of the SFX audio bus.
	/// </summary>
	public static int SFXAudioBusIndex = AudioServer.GetBusIndex(SFXAudioBusName);
	
	/// <summary>
	/// The name of the Voice audio bus.
	/// </summary>
	[Export]
	public static string VoiceAudioBusName = "Voice";

	/// <summary>
	/// The index of the voice audio bus.
	/// </summary>
	public static int VoiceAudioBusIndex = AudioServer.GetBusIndex(VoiceAudioBusName);

	public static float MusicVolume, SFXVolume, VoiceVolume;
	public static bool allVolumeMuted = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		voicePlayer = GetNode<AudioStreamPlayer>("VoicePlayer");
		footstepPlayer = GetNode<AudioStreamPlayer>("StepAudio");
		oneShotPlayer = GetNode<AudioStreamPlayer>("OneShotOther");
		bgMusicPlayer = GetNode<AudioStreamPlayer>("BGMusic");
		stepTimer = GetNode<Timer>("StepTimer");
		LoadAudioSettings();
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

	/// <summary>
	/// Change the value of the volume mute state to <c>mvalue</c>.
	/// </summary>
	/// <param name="mvalue"></param>
	public static void ChangeMute(bool mvalue) {
		allVolumeMuted = mvalue;
		
		// If mvalue is true, set the volume to 0.
		// Otherwise, scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(MusicAudioBusIndex, Mathf.LinearToDb(mvalue ? 
			0 : 
			MusicVolume >= 1 ? MusicVolume * MusicVolume : MusicVolume));
		AudioServer.SetBusVolumeDb(SFXAudioBusIndex, Mathf.LinearToDb(mvalue ? 
			0 : 
			SFXVolume >= 1 ? SFXVolume * SFXVolume : SFXVolume));
		AudioServer.SetBusVolumeDb(VoiceAudioBusIndex, Mathf.LinearToDb(mvalue ? 
			0 : 
			VoiceVolume >= 1 ? VoiceVolume * VoiceVolume : VoiceVolume));
		
		SaveAudioSettings();
	}

	/// <summary>
	/// Change the <c>type</c> volume level to <c>value</c>.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	public static void ChangeVolume(string type, float value) {
		float nvolume = Mathf.LinearToDb(value >= 1 ? value * value : value);
		switch (type)
		{
			case "music":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				AudioServer.SetBusVolumeDb(MusicAudioBusIndex, nvolume);
				MusicVolume = value;
				break;
			case "sfx":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				AudioServer.SetBusVolumeDb(SFXAudioBusIndex, nvolume);
				SFXVolume = value;
				break;
			case "voice":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				AudioServer.SetBusVolumeDb(VoiceAudioBusIndex, nvolume);
				VoiceVolume = value;
				break;
			default:
				break;
		}
		SaveAudioSettings();
	}

	/// <summary>
	/// Load audio settings for the game.
	/// </summary>
	public static void LoadAudioSettings() {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + "settings.json", Godot.FileAccess.ModeFlags.Read);
		var settingsData = JsonSerializer.Deserialize<Dictionary<string, object>>(file.GetAsText(), Globals.options);
		ChangeVolume("music", float.Parse(settingsData["MusicVolume"].ToString()));
		ChangeVolume("sfx", float.Parse(settingsData["SFXVolume"].ToString()));
		ChangeVolume("voice", float.Parse(settingsData["VoiceVolume"].ToString()));
		ChangeMute(bool.Parse(settingsData["VolumeMuted"].ToString()));
		// GD.Print(allVolumeMuted);
		file.Close();
	}
	
	/// <summary>
	/// Save audio settings for the game.
	/// </summary>
	public static void SaveAudioSettings() {
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + "settings.json", Godot.FileAccess.ModeFlags.Read);
		Dictionary<string, object> settingsData = new()
        {
            ["MusicVolume"] = MusicVolume,
            ["SFXVolume"] = SFXVolume,
            ["VoiceVolume"] = VoiceVolume,
            ["VolumeMuted"] = allVolumeMuted
        };
		string d = JsonSerializer.Serialize(settingsData, Globals.options);
		File.WriteAllText("Data/settings.json", d);
		file.Close();
	}

	void OnTimerTimeout() {
	}
}
