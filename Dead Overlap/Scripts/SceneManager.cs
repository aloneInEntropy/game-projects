using System;
using Godot;

public partial class SceneManager : Node
{
    /// <summary>
	/// Load a scene asyncronously. This uses the FadeOut animation in the current Location to work. <br/>
	/// If you need to load a scene statically, use SetScene.
	/// </summary>
	/// <param name="sceneName"></param>
	public async void LoadScene(string sceneName) {
		GameManager.isGamePaused = false;
		GameManager.canPauseGame = false;
		Globals.currentLocation.Leave();
		await ToSignal(Globals.currentLocation.animationPlayer, "animation_finished");
		Globals.currentLocation.GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
	}
	
	/// <summary>
	/// Load a scene. Uses a tether node to access the SceneTree. <br/>
	/// </summary>
	/// <param name="sceneName"></param>
	public static void SetScene(Node tether, string sceneName) {
		if (tether.IsInsideTree()) {
			tether.GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
			GameManager.gameResumed = true;
		}
	}

	/// <summary>
	/// Load a scene <c>sceneName</c>.
	/// </summary>
	/// <param name="sceneName"></param>
	public static void ChangeScene(string sceneName) {
		// GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
	}

	/// <summary>
	/// Load a scene <c>sceneName</c>.
	/// </summary>
	/// <param name="sceneName"></param>
	public void NSChangeScene(string sceneName) {
		GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
	}

}