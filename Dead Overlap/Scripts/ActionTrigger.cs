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

    [ExportGroup("Requirements")]
    [Export]
    public string triggerRequirementVariableName;
    [Export]
    public bool triggerRequirementVariableValue;
    [Export]
    public string triggerDeniedTextSource;
    [Export]
    public string triggerDeniedTextSpeaker;

    bool firstShown = false;


    /// <summary>
    /// Trigger this ActionTrigger object.
    /// </summary>
    public void Trigger() {
        if ((triggerRequirementVariableName is null) || 
			(triggerRequirementVariableName is not null && PlayerVariables.GetCheck(triggerRequirementVariableName) == triggerRequirementVariableValue)) {
            if (interactable) {
                OpenDescription(describer, descriptionPath);
            } else if (roomTrigger) {
                TryUpdateFacingPos(Globals.player.lastDirection);
                Change();
            } else {
                GD.Print($"Empty trigger {Name} @ {GetParent().Name}");
            }
		} else {
            OpenDescription(triggerDeniedTextSpeaker, triggerDeniedTextSource);
        }
    }

    /// <summary>
    /// Open the description for this Trigger's Interactable settings and describe it. <br/>
    /// If <c>source</c> is null, dialogue won't be displayed on requirement failure. <br/>
    /// The <c><paramref name="speaker"/></c> voice and portrait for the dialogue box when opened is the Narrator by default.
    /// </summary>
    /// <param name="describer"></param>
    public void OpenDescription(string speaker, string source) {
        if (source is not null) {
            Globals.player.Velocity = Vector2.Zero;
            Globals.talkingNPCName = speaker ?? "Narrator";
            Globals.talkingNPC = Globals.GetNPC(speaker) ?? Globals.GetNPC("Narrator");
            if (!firstShown) {
                (Globals.GetNPC(speaker) ?? Globals.GetNPC("Narrator")).LoadDialogue("Interactables/" + source);
                Globals.gui.db.Modify(speaker ?? "Narrator");
                firstShown = true;
            }
            if (!Globals.gui.ProgressDialogue(Globals.GetNPC(speaker) ?? Globals.GetNPC("Narrator"))) {
                firstShown = false;
            }
        }
    }


    /// <summary>
	/// Change the scene according to this Trigger's Room Trigger settings. <br/>
	/// If <c>keepXPosition</c> or <c>keepYPosition</c> are set to <c>true</c>, the <c>entryPoint</c> will be ignored.
	/// </summary>
	public void Change() {
        GameManager.sceneChangeFacing = facingDirection;
        GameManager.sceneChangePosition = new Vector2(
            keepXPosition ? Globals.player.Position.X : entryPoint.X,
            keepYPosition ? Globals.player.Position.Y : entryPoint.Y
        );
		SceneManager s = new();
        s.LoadScene(sceneName);
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
            if (autoTrigger) {
                Trigger();
            }
        }
    }
}