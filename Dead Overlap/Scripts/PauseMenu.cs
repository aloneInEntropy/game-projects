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
		

		// Load saved settings
		LoadSettings();
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
		AudioManager.ChangeMute(mvalue);
		globalVolumeMuteButton.ButtonPressed = mvalue;
		MusicVolumeSlider.Editable = !mvalue;
		SFXVolumeSlider.Editable = !mvalue;
		VoiceVolumeSlider.Editable = !mvalue;
	}

	/// <summary>
	/// Change the <c>type</c> volume level to <c>value</c>.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	public void ChangeVolume(string type, float value) {
		AudioManager.ChangeVolume(type, value);
		switch (type)
		{
			case "music":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				MusicVolumeSlider.Value = value;
				MusicVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
				break;
			case "sfx":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				SFXVolumeSlider.Value = value;
				SFXVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
				break;
			case "voice":
				// Scale volume quadratically if larger than 1, and normally otherwise.
				VoiceVolumeSlider.Value = value;
				VoiceVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
				break;
			default:
				break;
		}	
	}

	/// <summary>
	/// Load settings for the game.
	/// </summary>
	public void LoadSettings() {
		ChangeVolume("music", AudioManager.MusicVolume);
		ChangeVolume("sfx", AudioManager.SFXVolume);
		ChangeVolume("voice", AudioManager.VoiceVolume);
		ChangeMute(AudioManager.allVolumeMuted);
	}


	void OnResumeButtonPressed() {
		GameManager.TogglePause(GameManager.GAME_PAUSE_MODE_NEG);
	}
	
	void OnSettingsButtonPressed() {
		GameManager.TogglePause(GameManager.GAME_PAUSE_MODE_SETTINGS);
	}
	
	void OnQuitButtonPressed() {
		GameManager gm = new();
		gm.SaveAndQuit(this);
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
