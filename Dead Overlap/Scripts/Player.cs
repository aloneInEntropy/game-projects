using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// The Player class. Contains references and methods relating to the Player character.
/// </summary>
public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement")]
	[Export]
	public float Speed = 200.0f;
	[Export]
	private Vector2 _velocity;
	[Export]
	public float _friction = 1000;
	[Export]
	public float _acceleration = 800;

	/// <summary>
	/// The direction the Player is looking in. Determines walk sprite.
	/// </summary>
	public Vector2 direction;

	/// <summary>
	/// The last direction the Player was looking in before stopping movement (direction == (0, 0)). Determines idle sprite.
	/// </summary>
	public Vector2 lastDirection;

	public Area2D interactBox;
	RayCast2D rayCast;
	Area2D attackBox;
	CollisionShape2D attackBoxColl;
	AnimatedSprite2D sprite2D;
	AnimationPlayer animationPlayer;
	AnimationTree animationTree;
    readonly Array<Node2D> overlapping = new();
	GUI gui;
    Node2D facingObj;

	public override void _Ready() {
		Globals.player = this;
        rayCast = GetNodeOrNull<RayCast2D>("RayCast2D");
        sprite2D = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		interactBox = GetNodeOrNull<Area2D>("InteractBox");
		attackBox = GetNodeOrNull<Area2D>("AttackBox");
		attackBoxColl = GetNodeOrNull<CollisionShape2D>("AttackBox/CollisionShape2D");
        animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        animationTree = GetNodeOrNull<AnimationTree>("AnimationTree");
		animationTree.Active = true;
		gui = GetParent().GetNodeOrNull<GUI>("GUI");
		gui.Visible = true;
	}


    public override void _Process(double delta)
    {
		if (!GameManager.isGamePaused) {
			rayCast.TargetPosition = lastDirection * 32;
			attackBoxColl.Position = lastDirection * 64;
		}
		UpdateAnimationParameters();
    }

    public override void _Input(InputEvent @event)
    {
		if (!GameManager.isGamePaused && (@event.IsActionPressed("interact") || @event.IsActionPressed("next"))) {
			// if the player is overlapping multiple objects, choose the one it is looking at.
			// otherwise, pick the overlapping object.

			facingObj = overlapping.Count == 1 ?
				overlapping[0] :
				(Node2D)rayCast.GetCollider() // cast Object to Node2D
				// overlapping[^1]
			;

			if (facingObj is not null) {
				Velocity = Vector2.Zero; // Stop any sliding when talking to an NPC.
				if (facingObj.GetType().IsSubclassOf(typeof(NPC))) {
					Globals.talkingNPC = (NPC)facingObj;
					// GD.Print(facingObj.Name);
					if (DialogueManager.isDialogueReading) {
						// if dialogue is currently being typed out
						DialogueManager.UpdateVisibleText(true);
					} else {
						// if dialogue has finished being typed out
						gui.ProgressDialogue((NPC)facingObj);
					}
				} else if (facingObj.GetType() == typeof(ActionTrigger)) {
					if (DialogueManager.isDialogueReading) {
						// if dialogue is currently being typed out
						DialogueManager.UpdateVisibleText(true);
					} else {
						// if dialogue has finished being typed out
						((ActionTrigger)facingObj).Trigger();
					}
				}
			}
		}
    }
    

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;

		Speed = Input.IsActionPressed("sprint") ? 850f : 200f;

        direction = (gui.isDialogueActive || GameManager.isGamePaused) ? 
			Vector2.Zero :	
			Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
				
		// Velocity handling
		if (direction == Vector2.Zero) {
			_velocity = Vector2.Zero;
		} else {
			AudioManager.PlayStep();
			lastDirection = direction;
			_velocity = direction * Speed;
		}

		Velocity = _velocity;
		MoveAndSlide();
	}

	/// <summary>
	/// Update the animation parameters in the animation tree for the Player.
	/// </summary>
	void UpdateAnimationParameters() {
		animationPlayer.SpeedScale = 2.5f;
		animationTree.Set("parameters/conditions/idle", Velocity == Vector2.Zero);
		animationTree.Set("parameters/conditions/isMoving", Velocity != Vector2.Zero);
		animationTree.Set("parameters/Idle/blend_position", lastDirection);
		animationTree.Set("parameters/Walk/blend_position", lastDirection);
	}


	void _on_interact_box_area_entered(Area2D area) {
		if (area.GetParent().GetType().IsSubclassOf(typeof(NPC))) {
			// if the node is an NPC
			if (!overlapping.Contains(area.GetParent<NPC>())) overlapping.Add(area.GetParent<NPC>());
		} else if (area.GetType() == typeof(ActionTrigger)) {
			// if the node is an ActionTrigger
			if (!overlapping.Contains(area)) overlapping.Add(area);
		} 
	}
	
	void _on_interact_box_area_exited(Area2D area) {
		// GD.Print(area.GetParent<Node2D>().Name);
		if (area.GetParent().GetType().IsSubclassOf(typeof(NPC))) {
			gui.CloseDialogue();
			// if the node is an NPC
			if (overlapping.Contains(area.GetParent<NPC>())) overlapping.Remove(area.GetParent<NPC>());
		} else if (area.GetType() == typeof(ActionTrigger)) {
			// if the node is an ActionTrigger
			if (overlapping.Contains(area)) overlapping.Remove(area);
		}
	}

	void _on_animation_tree_animation_started(string anim_name) {
		if (anim_name[..6] == "attack")
		{
			attackBox.Monitoring = true;
			// GD.Print($"{anim_name} started");
		}
	}
	
	void _on_animation_tree_animation_finished(string anim_name) {
		if (anim_name[..6] == "attack")
		{
			attackBox.Monitoring = false;
			// GD.Print($"{anim_name} ended");
		}
	}

	void _on_attack_box_area_entered(Area2D area) {
		GD.Print($"{area.GetParent().Name} entered");
	}
	
	void _on_attack_box_area_exited(Area2D area) {
		GD.Print($"{area.GetParent().Name} exited");
	}
}
