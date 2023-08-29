using Godot;
using System;

/// <summary>
/// The ActionTrigger class. This is a node that will trigger depending on certain situations the Player is in, the primary of which is their <br/>
/// physical interaction with the CollisionPolygon2D this node has. <br/><br/>
/// If the setting <c>autoTrigger</c> is true, the class will trigger immediately upon touch. Otherwise, the Player must interact with the node to <br/>
/// trigger it.
/// </summary>
public partial class ActionTrigger : Area2D
{
    /// <summary>
    /// Is this ActionTrigger an interactable (display a dialogue box)?
    /// </summary>
    [Export]
    public bool interactable;

    /// <summary>
    /// Is this ActionTrigger a room trigger (changes rooms)?
    /// </summary>
    [Export]
    public bool roomTrigger;

    /// <summary>
    /// Should the ActionTrigger trigger immediately upon touch?
    /// </summary>
    [Export]
    public bool autoTrigger = false;

    /// <summary>
	/// The path to the file to parse when this ActionTrigger is triggered.
	/// </summary>
    [ExportGroup("Interactable")]
    [Export(PropertyHint.MultilineText)]
    public string descriptionPath = "";

    [Export]
    public string describer;

    [Export]
    public Texture showcase;

    public bool displayShowcase = false;

    /// <summary>
	/// The path to the PackedScene to load when this RoomTrigger is activated.
	/// </summary>
    [ExportGroup("Room Traversal")]
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
    /// Should the Player's position on the x-axis be kept when changing rooms?
    /// </summary>
    [Export]
    public bool keepXPosition = false;

    /// <summary>
    /// Should the Player's position on the y-axis be kept when changing rooms?
    /// </summary>
    [Export]
    public bool keepYPosition = false;

    [ExportSubgroup("Interaction")]
    [Export]
    public string entryRequirementVariableName;
    [Export]
    public bool entryRequirementVariableValue;
    [Export]
    public string entryDeniedTextSource;
    [Export]
    public string entryDeniedTextSpeaker;


    /// <summary>
    /// Trigger this ActionTrigger object, depending on whether it is an interactable (displays a dialogue box) or a room trigger (changes rooms).
    /// </summary>
    public void Trigger() {
        if (interactable) {
            OpenDescription();
        } else if (roomTrigger) {
            TryUpdateFacingPos(Globals.player.lastDirection);
            Change();
        } else {
            GD.Print($"Empty trigger {Name} @ {GetParent().Name}");
        }
    }

    /// <summary>
    /// Open the description for this Trigger's Interactable settings and describe it. <br/>
    /// The voice and portrait for the dialogue box when opened is the Narrator by default.
    /// </summary>
    /// <param name="describer"></param>
    public void OpenDescription() {
        Globals.talkingNPC = Globals.GetNPC("Narrator");
        Globals.gui.ProgressDialogue(Globals.GetNPC("Narrator"));
        Globals.gui.db.Modify(describer ?? "Narrator");
    }


    /// <summary>
	/// Change the scene according to this Trigger's Room Trigger settings. <br/>
	/// If <c>keepXPosition</c> or <c>keepYPosition</c> are set to <c>true</c>, the <c>entryPoint</c> will be ignored.
	/// </summary>
	public void Change() {
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
	/// This mainly applies to Auto Trigger room triggers. If the (Auto) Trigger would leave the Player facing an arbitrary position 
    /// (typically (0, 0)) instead of <br/>
	/// a specific position, this object's facingDirection will be updated to that value. Otherwise, it will prioritise the given direction <br/>
	/// set in the Godot Editor.
	/// </summary>
	/// <param name="facing"></param>
	public void TryUpdateFacingPos(Vector2 facing) {
		if (facingDirection == Vector2.Zero) facingDirection = facing;
	}

    public void OnAreaEntered(Area2D area) {
        if (area.GetParent().GetType() == typeof(Player)) {
            if (interactable) Globals.GetNPC("Narrator").LoadDialogue("Interactables/" + descriptionPath);
            if (entryRequirementVariableName is not null) Globals.GetNPC("Narrator").LoadDialogue("Interactables/" + entryDeniedTextSource);
            // else GD.Print("Nothing in this interactable");

            if (autoTrigger) {
                Globals.player.Velocity = Vector2.Zero;
                Trigger();
            }
        }
    }
}