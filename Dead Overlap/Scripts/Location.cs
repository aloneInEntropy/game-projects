using Godot;
using System;

public partial class Location : Node2D
{
	[Export]
	public Vector2 defaultEntryPoint;
	Player player = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetNode<Player>("Player");
		if (GameManager.sceneChangePosition != Vector2.Inf) {
			player.Position = GameManager.sceneChangePosition;
			GameManager.sceneChangePosition = Vector2.Inf;	
		} else {
			player.Position = defaultEntryPoint;
		}
		player.direction = GameManager.sceneChangeFacing;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
