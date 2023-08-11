using Godot;
using System;

// The Game Manager script.
public partial class GameManager : Node
{
	public static int frame = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		frame++;
	}

	// Check if a Type `potentialDescendant` is inherited from or equal to a class `potentialBase`
	// https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object
	public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) {
		return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
	}

	public static bool Inherits(Type potential, Type basec) {
		return potential.IsSubclassOf(basec);
	}
}
