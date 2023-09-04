using Godot;
using System;

public partial class Location : Node2D
{
	public Sprite2D foreground = new();
	[Export]
	public Vector2 defaultEntryPoint;
	public AnimationPlayer animationPlayer = new();
	Player player = new();
	TileMap tilemap = new();
	Camera2D camera = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Globals.currentLocation = this;
		
		player = GetNode<Player>("Player");
		camera = GetNode<Camera2D>("Player/Camera2D");
		tilemap = GetNode<TileMap>("TileMap");
		foreground = GetNode<Sprite2D>("Foreground");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		// Fade in animation
		foreground.Visible = true;
		foreground.Modulate = new Color(0, 0, 0, 1);
		animationPlayer.Play("FadeIn");

		// If the scene change position from the triggered ActionTrigger does not specify a position, 
		// default to this room's default entry point. 
		if (GameManager.sceneChangePosition != Vector2.Inf) {
			player.Position = GameManager.sceneChangePosition;
			GameManager.sceneChangePosition = Vector2.Inf;
		} else {
			player.Position = defaultEntryPoint;
		}

		// Change the Player's facing direction if it is not arbitrary.
		if (GameManager.sceneChangeFacing != Vector2.Zero) {
			player.lastDirection = GameManager.sceneChangeFacing;
		}

		// Set camera limits to the tilemap's used rect.
		var lims = tilemap.GetUsedRect();
		camera.LimitTop = lims.Position.Y * tilemap.CellQuadrantSize;
		camera.LimitBottom = (lims.Position.Y + lims.Size.Y) * tilemap.CellQuadrantSize;
		camera.LimitLeft = lims.Position.X * tilemap.CellQuadrantSize;
		camera.LimitRight = (lims.Position.X + lims.Size.X) * tilemap.CellQuadrantSize;
		
		GameManager.canPauseGame = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Leave() {
		animationPlayer.Play("FadeOut");
	}
}
