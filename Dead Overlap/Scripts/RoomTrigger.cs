using Godot;
using System;

public partial class RoomTrigger : Area2D
{
    /// <summary>
    /// The path to the PackedScene to load when this RoomTrigger is activated.
    /// </summary>
    [Export]
    public string sceneName;

    /// <summary>
    /// The position at which to load the Player upon scene change.
    /// </summary>
    [Export]
    public Vector2 entryPoint;
    
    /// <summary>
    /// The angle at which to face the Player upon scene change.
    /// </summary>
    [Export]
    public Vector2 faceDir;
    
    /// <summary>
    /// Should the next room be loaded immediately upon touch?
    /// </summary>
    [Export]
    public bool autoTrigger = false;

    /// <summary>
    /// Change the scene according to this RoomTrigger's variables.
    /// </summary>
    public void Change() {
		GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
		GameManager.sceneChangePosition = entryPoint;
		GameManager.sceneChangeFacing = faceDir;
    }
}
