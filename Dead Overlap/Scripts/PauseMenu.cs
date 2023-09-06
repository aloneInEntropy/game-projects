using Godot;
using System;

public partial class PauseMenu : Control
{
	public Control settings = new();
	public VBoxContainer settingsContainer = new();
	public HBoxContainer volumeContainer = new();
	public HSlider volumeSlider = new();
	public HBoxContainer musicVolumeContainer = new();
	public HSlider musicVolumeSlider = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		settings = GetNode<Control>("Settings");
		settingsContainer = settings.GetNode<VBoxContainer>("SettingsContainer");
		volumeContainer = settingsContainer.GetNode<HBoxContainer>("Volume");
		volumeSlider = volumeContainer.GetNode<HSlider>("Control/HSlider");
		volumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.globalAudioBusIndex));
		volumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(volumeSlider.Value)}[/center]";
		
		musicVolumeContainer = settingsContainer.GetNode<HBoxContainer>("Volume");
		musicVolumeSlider = musicVolumeContainer.GetNode<HSlider>("Control/HSlider");
		musicVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioManager.musicAudioBusIndex));
		musicVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(musicVolumeSlider.Value)}[/center]";
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
		GetTree().Quit();
	}

	void OnGlobalVolumeValueChanged(float value) {
		// Scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(AudioManager.globalAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value*value : value));
		volumeSlider.Value = value;
		AudioManager.masterVolume = value;
		volumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
	}
	
	void OnMusicVolumeValueChanged(float value) {
		// Scale volume quadratically if larger than 1, and normally otherwise.
		AudioServer.SetBusVolumeDb(AudioManager.musicAudioBusIndex, Mathf.LinearToDb(value >= 1 ? value*value : value));
		musicVolumeSlider.Value = value;
		// AudioManager.masterVolume = value;
		musicVolumeContainer.GetNode<RichTextLabel>("Control/Amount").Text = $"[center]{Mathf.Abs(value)}[/center]";
	}
}
