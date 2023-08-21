using Godot;
using System;
using System.Text.RegularExpressions;


/// <summary>
/// The Game Manager script. Handles all general/auxillary functions and variables.
/// </summary>
public partial class GameManager : Node
{
	public static int frame = 0;

	public static Font normalFont, boldFont, lightFont;

	public static bool isGamePaused = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		normalFont = GD.Load<Font>("res://Assets/Fonts/futur.ttf");
		boldFont = GD.Load<Font>("res://Assets/Fonts/Futura Extra Black font.ttf");
		lightFont = GD.Load<Font>("res://Assets/Fonts/futura light bt.ttf");
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

	/// <summary>
	/// Remove any BBCode tags from the string <c>s</c>.
	/// </summary>
	/// <param name="s"></param>
	/// <returns>The string with all BBCode tags removed.</returns>
	public static string RemoveBBCTags(string s) {
		return Regex.Replace(s, "\\[.*?\\]", "");
	}
}
