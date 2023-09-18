using Godot;
using System;
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

	List<double> volumes = new();

	public override void _Ready()
	{
		Visible = false;
		settings = GetNode<Control>("Settings");
		settingsContainer = settings.GetNode<VBoxContainer>("SettingsContainer");
		volumes.Add(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.musicAudioBusIndex)));
		volumes.Add(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.SFXAudioBusIndex)));
		volumes.Add(Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.voiceAudioBusIndex)));

		// Mute all volume check box
		globalVolumeContainer = settingsContainer.GetNode<HBoxContainer>("GlobalVolume");
		globalVolumeMuteButton = globalVolumeContainer.GetNode<CheckButton>("Control/CheckButton");
		
		// Music volume slider 
		MusicVolumeContainer = settingsContainer.GetNode<HBoxContainer>("MusicVolume");
		MusicVolumeSlider = MusicVolumeContainer.GetNode<HSlider>("Control/HSlider");
		MusicVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.musicAudioBusIndex));
		MusicVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(MusicVolumeSlider.Value)}[/center]";
		
		// SFX volume slider 
		SFXVolumeContainer = settingsContainer.GetNode<HBoxContainer>("SFXVolume");
		SFXVolumeSlider = SFXVolumeContainer.GetNode<HSlider>("Control/HSlider");
		SFXVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.SFXAudioBusIndex));
		SFXVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(SFXVolumeSlider.Value)}[/center]";
		
		// NPC Voice volume slider 
		VoiceVolumeContainer = settingsContainer.GetNode<HBoxContainer>("VoiceVolume");
		VoiceVolumeSlider = VoiceVolumeContainer.GetNode<HSlider>("Control/HSlider");
		VoiceVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.voiceAudioBusIndex));
		VoiceVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(VoiceVolumeSlider.Value)}[/center]";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void OnResumeButtonPressed() {
		GameManager.TogglePause(GameManager.GAME_PAUSE_MODE_NEG);
	}
	
	void OnSettingsButtonPressed() {
		GameManager.TogglePause(GameManager.GAME_PAUSE_MODE_SETTINGS);
	}
	
	void OnQuitButtonPressed() {
		PlayerVariables.Save();
		GetTree().Quit();
	}

	void OnMuteButtonToggled(bool button_pressed) {
		if (button_pressed) {
			AudioServer.SetBusVolumeDb(AudioManager.musicAudioBusIndex, Mathf.LinearToDb(0));
			AudioServer.SetBusVolumeDb(AudioManager.SFXAudioBusIndex, Mathf.LinearToDb(0));
			AudioServer.SetBusVolumeDb(AudioManager.voiceAudioBusIndex, Mathf.LinearToDb(0));
			MusicVolumeSlider.Editable = false;
			SFXVolumeSlider.Editable = false;
			VoiceVolumeSlider.Editable = false;
		} else {
			AudioServer.SetBusVolumeDb(AudioManager.musicAudioBusIndex, Mathf.LinearToDb((float)(volumes[0] >= 1 ? volumes[0]*volumes[0] : volumes[0])));
			AudioServer.SetBusVolumeDb(AudioManager.SFXAudioBusIndex, Mathf.LinearToDb((float)(volumes[1] >= 1 ? volumes[1]*volumes[1] : volumes[1])));
			AudioServer.SetBusVolumeDb(AudioManager.voiceAudioBusIndex, Mathf.LinearToDb((float)(volumes[2] >= 1 ? volumes[2]*volumes[2] : volumes[2])));
			MusicVolumeSlider.Editable = true;
			SFXVolumeSlider.Editable = true;
			VoiceVolumeSlider.Editable = true;
		}
	}

	void OnMusicVolumeValueChanged(float value) {
		// Scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(AudioManager.musicAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value * value : value));
		MusicVolumeSlider.Value = value;
		volumes[0] = value;
		MusicVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
	}

	void OnSFXVolumeValueChanged(float value) {
		// Scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(AudioManager.SFXAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value * value : value));
		SFXVolumeSlider.Value = value;
		volumes[1] = value;
		SFXVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
	}
	
	void OnVoiceVolumeValueChanged(float value) {
		// Scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(AudioManager.voiceAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value * value : value));
		VoiceVolumeSlider.Value = value;
		volumes[2] = value;
		VoiceVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
	}
}
