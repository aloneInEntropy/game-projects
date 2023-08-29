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

	[ExportGroup("Interaction")]
	[Export]
	public string entryRequirementVariableName;
	[Export]
	public bool entryRequirementVariableValue;
	[Export]
	public string entryDeniedTextSource;
	[Export]
	public string entryDeniedTextSpeaker;

	/// <summary>
	/// Change the scene according to this RoomTrigger's variables. <br/>
	/// If <c>keepXPosition</c> or <c>keepYPosition</c> are set to <c>true</c>, the <c>entryPoint</c> will be ignored.
	/// </summary>
	public virtual void Change() {
		// GameManager.sceneChangeFacing = facingDirection;
		// GameManager.sceneChangePosition = new Vector2(
		// 	keepXPosition ? Globals.player.Position.X : entryPoint.X,
		// 	keepYPosition ? Globals.player.Position.Y : entryPoint.Y
		// );
		// GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
		if ((entryRequirementVariableName is null) || 
			(entryRequirementVariableName is not null && (bool)PlayerVariables.GetVar(entryRequirementVariableName) == entryRequirementVariableValue)) {
			GameManager.sceneChangeFacing = facingDirection;
			GameManager.sceneChangePosition = new Vector2(
				keepXPosition ? Globals.player.Position.X : entryPoint.X,
				keepYPosition ? Globals.player.Position.Y : entryPoint.Y
			);
			GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
		} else {
			// GD.Print("cant go in");
			Globals.talkingNPC = Globals.GetNPC("Narrator");
            Globals.gui.ProgressDialogue(Globals.GetNPC("Narrator"));
            Globals.gui.db.Modify(entryDeniedTextSpeaker ?? "Narrator");
		}
	}

	/// <summary>
	/// Attempt to update the facing position for this RoomTrigger object.<br/>
	/// This mainly applies to Auto Trigger RoomTriggers. If the (Auto) Trigger would leave the Player facing an arbitrary position instead of <br/>
	/// a specific position, the this object's facingDirection will be updated to that value. Otherwise, it will prioritise the given direction <br/>
	/// set in the Godot Editor.
	/// </summary>
	/// <param name="facing"></param>
	public void TryUpdateFacingPos(Vector2 facing) {
		if (facingDirection == Vector2.Zero) facingDirection = facing;
	}

	public void OnAreaEntered(Area2D area) {
        if (area.GetParent().GetType() == typeof(Player)) {
            if (entryDeniedTextSource is not null) Globals.GetNPC("Narrator").LoadDialogue("Interactables/" + entryDeniedTextSource);
			else GD.Print("cant go in");
        }
    }
}
