using Godot;
using System;

public partial class Location : Node2D
{
	[Export]
	public Vector2 defaultEntryPoint;
	Player player = new();
	TileMap tilemap = new();
	Camera2D camera = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetNode<Player>("Player");
		camera = GetNode<Camera2D>("Player/Camera2D");
		tilemap = GetNode<TileMap>("TileMap");
		if (GameManager.sceneChangePosition != Vector2.Inf) {
			player.Position = GameManager.sceneChangePosition;
			GameManager.sceneChangePosition = Vector2.Inf;	
		} else {
			player.Position = defaultEntryPoint;
		}
		player.direction = GameManager.sceneChangeFacing;
		var lims = tilemap.GetUsedRect();
		// GD.Print(lims * tilemap.CellQuadrantSize);
		camera.LimitTop = lims.Position.Y * tilemap.CellQuadrantSize;
		camera.LimitBottom = (lims.Position.Y + lims.Size.Y) * tilemap.CellQuadrantSize;
		camera.LimitLeft = lims.Position.X * tilemap.CellQuadrantSize;
		camera.LimitRight = (lims.Position.X + lims.Size.X) * tilemap.CellQuadrantSize;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
