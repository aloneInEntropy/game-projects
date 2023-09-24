using Godot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


/// <summary>
/// The Game Manager script. Handles all general/auxillary functions and variables.
/// </summary>
public partial class GameManager : Node
{	
	/// <summary>
	/// The number of frames ran since the game started in Process.
	/// </summary>
	public static int frame = 0;
	
	/// <summary>
	/// Has the player paused the game?
	/// </summary>
	public static bool isGamePaused = false;
	
	/// <summary>
	/// Has the player opened the settings?
	/// </summary>
	public static bool isSettingsOpen = false;
	
	/// <summary>
	/// Can the player pause the game?
	/// </summary>
	public static bool canPauseGame = true;

	public static Font normalFont, boldFont, lightFont;
	public static Theme buttonTheme;

	public static Vector2 sceneChangePosition = Vector2.Inf;
	public static Vector2 sceneChangeFacing;

	/// <summary>
	/// Unpaused game state.
	/// </summary>
	public const int GAME_PAUSE_MODE_NEG = -1;
	/// <summary>
	/// Pause mode menu. Opens the game's pause menu.
	/// </summary>
	public const int GAME_PAUSE_MODE_MENU = 0;
	/// <summary>
	/// Pause mode settings. Opens the settings from the pause menu.
	/// </summary>
	public const int GAME_PAUSE_MODE_SETTINGS = 1;
	/// <summary>
	/// Pause mode notebook. Opens the Notebook.
	/// </summary>
	public const int GAME_PAUSE_MODE_NOTEBOOK = 2;

	/// <summary>
	/// The current pause mode of the game. -1 means the game is unpaused.
	/// </summary>
	public static int GamePauseMode = GAME_PAUSE_MODE_NEG;

	/// <summary>
	/// Is the game resuming and loading in its first scene?
	/// </summary>
	public static bool gameResumed = false;

	SignalBus sb = new();

	// [Signal]
	// public delegate void DataLoadedEventHandler();
	// [Signal]
	// public delegate void DataSavedEventHandler();
	[Signal]
	public delegate void SHEventHandler(string n);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		normalFont = GD.Load<Font>("res://Assets/Fonts/futur.ttf");
		boldFont = GD.Load<Font>("res://Assets/Fonts/Futura Extra Black font.ttf");
		lightFont = GD.Load<Font>("res://Assets/Fonts/futura light bt.ttf");
		buttonTheme = GD.Load<Theme>("res://Resources/ButtonTheme.tres");

		SH += SayHello;
		EmitSignal(SignalName.SH, "first");
		EmitSignal(SignalName.SH, "second");
		// AddUserSignal("SayHelloAgain");
		// DataSaved += () => GD.Print("Data Saved");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		frame++;
	}
	
	/// <summary>
	/// Toggle the pause state of the game and defines the logic for the screen to show. <br/>
	/// The value <c>pauseMode</c> is the current pause screen that appears if intending to pause the game.
	/// </summary>
	/// <param name="pauseMode"></param>
	/// <returns><c>true</c> if the game is paused, <c>false</c> otherwise.</returns>
	public static bool TogglePause(int pauseMode = GAME_PAUSE_MODE_MENU) {
		// If the game cannot be paused, return false.
		if (canPauseGame) {
			// will the game be paused or not?
            bool pausedOrNot; 
            if (!isGamePaused) {
				// if the game isn't paused, pause it
				isGamePaused = true;
				GamePauseMode = pauseMode;
				pausedOrNot = true;
			} else {
				// if the game is paused, control what toggling pause will do to the current screen
				switch (GamePauseMode) {
					case GAME_PAUSE_MODE_SETTINGS:
						// if the settings menu is open, retreat to the base pause menu
						GamePauseMode = GAME_PAUSE_MODE_MENU;
						pausedOrNot = true;
						break;
					case GAME_PAUSE_MODE_MENU:
						if (pauseMode == GAME_PAUSE_MODE_SETTINGS) {
							// If the settings are opened, switch to the settings pause state
							GamePauseMode = GAME_PAUSE_MODE_SETTINGS;
							pausedOrNot = true;
							isSettingsOpen = true;
						} else {
							// Otherwise, fall down to the Notebook case
							goto case GAME_PAUSE_MODE_NOTEBOOK;
						}
						break;
					case GAME_PAUSE_MODE_NOTEBOOK:
						// if the Notebook or base pause menus are open, unpause the game.
						GamePauseMode = GAME_PAUSE_MODE_NEG;
						pausedOrNot = false;
						break;
					default:
						return true;
				}
			}
			// display the pause menu for the current pause mode
			DisplayPauseMenu(GamePauseMode);

			// return whether or not the game is paused.
			return pausedOrNot;
		} 
		return false;
	}

	/// <summary>
	/// Display the pause menu for a given pause mode.
	/// </summary>
	/// <param name="pauseMode"></param>
	public static void DisplayPauseMenu(int pauseMode = GAME_PAUSE_MODE_MENU) {
		switch (pauseMode) {
			case GAME_PAUSE_MODE_NEG:
				isGamePaused = false;
				Globals.gui.notebook.Visible = false;
				Globals.gui.pauseMenu.Visible = false;
				GamePauseMode = GAME_PAUSE_MODE_NEG;
				// GD.Print("unpaused");
				break;
			case GAME_PAUSE_MODE_MENU:
				// GD.Print("pause menu open");
				Globals.gui.pauseMenu.Visible = true;
				Globals.gui.pauseMenu.settings.Visible = false;
				break;
			case GAME_PAUSE_MODE_SETTINGS:
				Globals.gui.pauseMenu.settings.Visible = true;
				// GD.Print("settings open");
				break;
			case GAME_PAUSE_MODE_NOTEBOOK:
				Globals.gui.notebook.Visible = true;
				// GD.Print("notebook open");
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Check if a Type `potentialDescendant` is inherited from or equal to a class `potentialBase` <br/>
	/// https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object
	/// </summary>
	/// <param name="potentialBase"></param>
	/// <param name="potentialDescendant"></param>
	/// <returns></returns>
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

	
	/// <summary>
	/// Loads saved Player and game data into Global and Player variables.
	/// </summary>
	public void LoadGameData(Node tether) {
		sb.DataLoaded += ConfirmLoad;
		PlayerVariables.LoadPlayerVariables(tether, "variables.json");
		PlayerVariables.LoadMissions("missions.json");
        PlayerVariables.LoadClues("clues.json");
		sb.EmitSignal("DataLoaded");
	}
	
	/// <summary>
	/// Save all Player and game data.
	/// </summary>
	public void SaveGameData() {
		sb.DataSaved += ConfirmSave;
		PlayerVariables.SavePlayerVariables("variables.json");
		PlayerVariables.SaveMissions("missions.json");
		PlayerVariables.SaveClues("clues.json");
		sb.EmitSignal("DataSaved");
		// @ts-ignore
		// EmitSignal(SignalName.DataSaved);
		// GD.Print("saved");
	}

	/// <summary>
	/// Save and quit the game.
	/// </summary>
	public void SaveAndQuit(Node tether) {
		SaveGameData();
		// await ToSignal(eb, "DataSaved");
		tether.GetTree().Quit();
	}

	public void SayHello(string name) {
		GD.Print($"hello {name}");
	}

	public void ConfirmLoad() {
		GD.Print("loaded");
	}
	public void ConfirmSave() {
		GD.Print("saved");
	}
}
