using Godot;
using Godot.Collections;


/// <summary>
/// The Player class. Contains references and methods relating to the Player character.
/// </summary>
public partial class Player : CharacterBody2D
{
	[ExportGroup("Movement")]
	[Export]
	public const float Speed = 300.0f;
	[Export]
	private Vector2 _velocity;
	[Export]
	public float _friction = 1000;
	[Export]
	public float _acceleration = 800;

	RayCast2D rayCast;
	Area2D attackBox;
	CollisionShape2D attackBoxColl;
	AnimatedSprite2D sprite2D;
	AnimationPlayer animationPlayer;
	AnimationTree animationTree;
    readonly Array<Node2D> overlapping = new();
	GUI gui;
    Node2D facingObj;
	Vector2 direction;
	Vector2 lastDirection;

	public override void _Ready() {
		Globals.player = this;
        rayCast = GetNodeOrNull<RayCast2D>("RayCast2D");
        sprite2D = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		attackBox = GetNodeOrNull<Area2D>("AttackBox");
		attackBoxColl = GetNodeOrNull<CollisionShape2D>("AttackBox/CollisionShape2D");
        animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        animationTree = GetNodeOrNull<AnimationTree>("AnimationTree");
		animationTree.Active = true;
		gui = GetParent().GetNodeOrNull<GUI>("GUI");
	}


    public override void _Process(double delta)
    {
		if (!GameManager.isGamePaused) {
			rayCast.TargetPosition = lastDirection * 32;
			attackBoxColl.Position = lastDirection * 64;
			UpdateAnimationParameters();
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (!GameManager.isGamePaused && (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("next"))) {
			// if the player is overlapping multiple objects, choose the one it is looking at.
			// otherwise, pick the overlapping object.

			facingObj = overlapping.Count == 1 ?
				overlapping[0] :
				(Node2D)rayCast.GetCollider() // cast Object to Node2D
			;
			// GD.Print(GameManager.IsSameOrSubclass(typeof(NPC), facingObj.GetType()));
			
			if (facingObj is not null && facingObj.GetType().IsSubclassOf(typeof(NPC))) {
				Velocity = Vector2.Zero; // Stop any sliding when talking to an NPC.
				Globals.talkingNPC = (NPC)facingObj;
				// GD.Print(facingObj.Name);
				if (DialogueManager.isDialogueReading) {
					// if dialogue is currently being typed out
					DialogueManager.UpdateVisibleText(true);
				} else {
					// if dialogue has finished being typed out
					gui.ProgressDialogue((NPC)facingObj);
				}
			}
		}
    }
    

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		direction = gui.isDialogueActive || GameManager.isGamePaused ? 
			Vector2.Zero :	
			Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		
		if (direction != Vector2.Zero) lastDirection = direction;

		// Velocity handling
		_velocity = direction == Vector2.Zero ? 
			_velocity.MoveToward(Vector2.Zero, _friction * (float)delta) :
			_velocity.MoveToward(direction * Speed, _acceleration * (float)delta);
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
		animationTree.Set("parameters/conditions/attack", Input.IsActionJustPressed("attack"));
		animationTree.Set("parameters/Idle/blend_position", lastDirection);
		animationTree.Set("parameters/Walk/blend_position", lastDirection);
		animationTree.Set("parameters/Attack/blend_position", lastDirection);
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
			if (!overlapping.Contains(area.GetParent<NPC>())) overlapping.Add(area.GetParent<NPC>());
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
			gui.CloseDialogue();
			// if the node is an NPC
			if (overlapping.Contains(area.GetParent<NPC>())) overlapping.Remove(area.GetParent<NPC>());
			// GD.Print(string.Format("{0} entered {1}", Name, area.GetParent<Node2D>().Name));
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
