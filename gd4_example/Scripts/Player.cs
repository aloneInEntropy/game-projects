using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement")]
	[Export]
	public const float Speed = 300.0f;
	[Export]
	private Vector2 _velocity;
	[Export]
	public float _friction = 600;
	[Export]
	public float _acceleration = 800;

    readonly Array<Node2D> overlapping = new();
	GUI gui;
    Node2D facingObj;
	RayCast2D rayCast;
	Vector2 direction;
	Vector2 lastDirection;

	public override void _Ready() {
        // GetTree().GetChild("");
		gui = GetParent().GetNodeOrNull<GUI>("GUI");
        rayCast = GetNodeOrNull<RayCast2D>("RayCast2D");
		gui.missionText.Text = "#######################################";
	}


    public override void _Process(double delta)
    {
		gui.missionText.Text = facingObj is null ? 
			"null" : 
			facingObj.Name
		;
		rayCast.TargetPosition = lastDirection * 32;
		// GD.Print(rayCast.TargetPosition);
		// GD.Print(rayCast.GetCollider());
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("next")) {
			// if the player is overlapping multiple objects, choose the one it is looking at.
			// otherwise, pick the overlapping object.

			facingObj = overlapping.Count == 1 ?
				overlapping[0] :
				(Node2D)rayCast.GetCollider() // cast Object to Node2D
			;
			// GD.Print(GameManager.IsSameOrSubclass(typeof(NPC), facingObj.GetType()));
			
			if (facingObj is not null && facingObj.GetType().IsSubclassOf(typeof(NPC))) {
				// GD.Print(facingObj.Name);
				if (DialogueManager.isDialogueActive) {
					// if dialogue is currently being typed out
					DialogueManager.UpdateVisibleText(true);
				} else {
					// if dialogue has finished being typed out
					gui.ProgressDialogue((NPC)facingObj);
					// if (gui.canProgressDialogue) gui.ShowDialogue(((NPC)facingObj).GetNextDialogue());
					// if (currentDiag < dobj.Count) {
					// 	// diags.RemoveAt(0);
					// } else {
					// 	gui.CloseDialogue();
					// }
				}
			}
		}
    }
    

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		direction = gui.isDialogueActive ? 
			Vector2.Zero :	
			Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero) lastDirection = direction;
		_velocity = direction == Vector2.Zero ? 
			_velocity.MoveToward(Vector2.Zero, _friction * (float)delta) :
			_velocity.MoveToward(direction * Speed, _acceleration * (float)delta)
		;
		Velocity = _velocity;
		MoveAndSlide();
	}

	#pragma warning disable IDE0051 // remove unused function INFO (this is ran by Godot internally anyway)
	#pragma warning disable IDE1006 // remove naming style INFO ("should start with a capital letter" stuff)
	void _on_interact_box_area_entered(Area2D area) {
		#pragma warning restore IDE0051 // restore the INFO (they're still useful after all)
		#pragma warning restore IDE1006 // ditto
		// GD.Print(area.GetParent<Node2D>().Name);
		if (area.GetParent().GetType().IsSubclassOf(typeof(NPC))) {
			// if the node is an NPC, start it's dialogue from the beginning
			// var p = (NPC)area.GetParent();
			// p.ResetDialogue();
			if (!overlapping.Contains(area.GetParent<Node2D>())) overlapping.Add(area.GetParent<Node2D>());
			// GD.Print(string.Format("{0} entered {1}", Name, area.GetParent<Node2D>().Name));
		}
	}
	
	#pragma warning disable IDE0051
	#pragma warning disable IDE1006
	void _on_interact_box_area_exited(Area2D area) {
		#pragma warning restore IDE0051
		#pragma warning restore IDE1006
		// GD.Print(area.GetParent<Node2D>().Name);
		if (area.GetParent().GetType().IsSubclassOf(typeof(NPC))) {
			// if the node is an NPC
			gui.CloseDialogue();
			if (overlapping.Contains(area.GetParent<Node2D>())) overlapping.Remove(area.GetParent<Node2D>());
			// GD.Print(string.Format("{0} entered {1}", Name, area.GetParent<Node2D>().Name));
		}
	}
}
