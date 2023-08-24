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
    public Vector2 facingDirection;
    
    /// <summary>
    /// Should the next room be loaded immediately upon touch?
    /// </summary>
    [Export]
    public bool autoTrigger = false;
    
    /// <summary>
    /// Should the Player's position on the x-axis be kept when changing rooms?
    /// </summary>
    [Export]
    public bool keepXPosition = false;
    
    /// <summary>
    /// Should the Player's position on the y-axis be kept when changing rooms?
    /// </summary>
    [Export]
    public bool keepYPosition = false;

    /// <summary>
    /// Change the scene according to this RoomTrigger's variables. <br/>
	/// If <c>keepXPosition</c> or <c>keepYPosition</c> are set to <c>true</c>, the <c>entryPoint</c> will be ignored.
    /// </summary>
    public void Change() {
      GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
		GameManager.sceneChangePosition = new Vector2(
		  keepXPosition ? Globals.player.Position.X : entryPoint.X,
		  keepYPosition ? Globals.player.Position.Y : entryPoint.Y
	  );
      GameManager.sceneChangeFacing = facingDirection;
    }
}
