using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;

public partial class PauseMenu : Control
{
	public Control settings = new();
	public VBoxContainer settingsContainer = new();
	public HBoxContainer globalVolumeContainer = new();
	public CheckButton globalVolumeMuteButton = new();
	public HBoxContainer MusicVolumeContainer = new();
	public HSlider MusicVolumeSlider = new();
	public HBoxContainer SFXVolumeContainer = new();
	public HSlider SFXVolumeSlider = new();
	public HBoxContainer VoiceVolumeContainer = new();
	public HSlider VoiceVolumeSlider = new();

	List<float> volumes = new();
	public bool allVolumeMuted = false;

	public override void _Ready()
	{
		/* Nodes */
		Visible = false;
		// Settings containter
		settings = GetNode<Control>("Settings");
		settingsContainer = settings.GetNode<VBoxContainer>("SettingsContainer");
		// Mute all volume check box
		globalVolumeContainer = settingsContainer.GetNode<HBoxContainer>("GlobalVolume");
		globalVolumeMuteButton = globalVolumeContainer.GetNode<CheckButton>("Control/CheckButton");
		// Music volume slider 
		MusicVolumeContainer = settingsContainer.GetNode<HBoxContainer>("MusicVolume");
		MusicVolumeSlider = MusicVolumeContainer.GetNode<HSlider>("Control/HSlider");
		// SFX volume slider 
		SFXVolumeContainer = settingsContainer.GetNode<HBoxContainer>("SFXVolume");
		SFXVolumeSlider = SFXVolumeContainer.GetNode<HSlider>("Control/HSlider");
		// NPC Voice volume slider 
		VoiceVolumeContainer = settingsContainer.GetNode<HBoxContainer>("VoiceVolume");
		VoiceVolumeSlider = VoiceVolumeContainer.GetNode<HSlider>("Control/HSlider");
		volumes.Add(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.musicAudioBusIndex)));
		volumes.Add(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.SFXAudioBusIndex)));
		volumes.Add(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.voiceAudioBusIndex)));

		// Load saved settings
		Godot.FileAccess file = Godot.FileAccess.Open(Globals.resPathToData + "settings.json", Godot.FileAccess.ModeFlags.Read);
		var settingsData = JsonSerializer.Deserialize<Dictionary<string, object>>(file.GetAsText(), Globals.options);
		ChangeVolume("music", int.Parse(settingsData["MusicVolume"].ToString()));
		ChangeVolume("sfx", int.Parse(settingsData["SFXVolume"].ToString()));
		ChangeVolume("voice", int.Parse(settingsData["VoiceVolume"].ToString()));
		ChangeMute(bool.Parse(settingsData["VolumeMuted"].ToString()));
		GD.Print(allVolumeMuted);
		file.Close();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Change the value of the volume mute state to <c>mvalue</c>.
	/// </summary>
	/// <param name="mvalue"></param>
	public void ChangeMute(bool mvalue) {
		allVolumeMuted = mvalue;
		globalVolumeMuteButton.ButtonPressed = mvalue;
		MusicVolumeSlider.Editable = !mvalue;
		SFXVolumeSlider.Editable = !mvalue;
		VoiceVolumeSlider.Editable = !mvalue;

		// If mvalue is true, set the volume to 0.
		// Otherwise, scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(AudioManager.musicAudioBusIndex, Mathf.LinearToDb(mvalue ? 
			0 : 
			volumes[0] >= 1 ? volumes[0] * volumes[0] : volumes[0]));
		AudioServer.SetBusVolumeDb(AudioManager.SFXAudioBusIndex, Mathf.LinearToDb(mvalue ? 
			0 : 
			volumes[1] >= 1 ? volumes[1] * volumes[1] : volumes[1]));
		AudioServer.SetBusVolumeDb(AudioManager.voiceAudioBusIndex, Mathf.LinearToDb(mvalue ? 
			0 : 
			volumes[2] >= 1 ? volumes[2] * volumes[2] : volumes[2]));
	}

	/// <summary>
	/// Change the <c>type</c> volume level to <c>value</c>.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	public void ChangeVolume(string type, float value) {
		switch (type)
		{
			case "music":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				AudioServer.SetBusVolumeDb(AudioManager.musicAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value * value : value));
				MusicVolumeSlider.Value = value;
				volumes[0] = value;
				MusicVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
				break;
			case "sfx":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				AudioServer.SetBusVolumeDb(AudioManager.SFXAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value * value : value));
				SFXVolumeSlider.Value = value;
				volumes[1] = value;
				SFXVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
				break;
			case "voice":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				AudioServer.SetBusVolumeDb(AudioManager.voiceAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value * value : value));
				VoiceVolumeSlider.Value = value;
				volumes[2] = value;
				VoiceVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
				break;
			default:
				break;
		}	
	}

	void OnResumeButtonPressed() {
		GameManager.TogglePause(GameManager.GAME_PAUSE_MODE_NEG);
	}
	
	void OnSettingsButtonPressed() {
		GameManager.TogglePause(GameManager.GAME_PAUSE_MODE_SETTINGS);
	}
	
	void OnQuitButtonPressed() {
		GameManager.SaveGameData();
		GetTree().Quit();
	}

	void OnMuteButtonToggled(bool button_pressed) {
		ChangeMute(button_pressed);
	}

	void OnMusicVolumeValueChanged(float value) {
		ChangeVolume("music", value);
	}

	void OnSFXVolumeValueChanged(float value) {
		ChangeVolume("sfx", value);
	}
	
	void OnVoiceVolumeValueChanged(float value) {
		ChangeVolume("voice", value);
	}
}
