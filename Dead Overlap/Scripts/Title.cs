using Godot;
using System;

public partial class Title : Control
{
	void OnResumeButtonPressed() {
		GD.Print("resuming");
		Resume();
	}

	void OnSettingsButtonPressed() {
		GD.Print("settings");
	}
	void OnNGButtonPressed() {
		GD.Print("new game");
	}
	void OnQuitButtonPressed() {
		GetTree().Quit();
	}

	void Resume() {
		// await GameManager.LoadData();
		// GameManager g = new();
		// g.LoadScene("TownSouth");
		GetTree().ChangeSceneToFile("res://Scenes/" + "TownSouth" + ".tscn");
	}
}
