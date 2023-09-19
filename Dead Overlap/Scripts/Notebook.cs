using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;



/// <summary>
/// The Notebook class. Contains information about information kept by Marceline Ren, including notes created by the Player.
/// </summary>
public partial class Notebook : Control
{
	public TabContainer tabContainer = new();
	Control suspectWindow = new();
	Control victimWindow = new();
	ScrollContainer noteWindow = new();
	ScrollContainer missionWindow = new();
	Control clueWindow = new();

	/// <summary>
	/// The RichTextLabel containing the description of the selected suspect.
	/// </summary>
	RichTextLabel suspectDescription = new();

	/// <summary>
	/// The VBoxContainer storing all saved notes as RichTextLabels and their respective HSeparators.
	/// </summary>
	VBoxContainer savedDialogueContainer = new();

	/// <summary>
	/// The VBoxContainer storing all missions as RichTextLabels and their respective HSeparators.
	/// </summary>
	VBoxContainer missionContainer = new();

	/// <summary>
	/// The GridContainer storing all clues as buttons.
	/// </summary>
	GridContainer clueContainer = new();
	
	/// <summary>
	/// The Control container storing the clue modal.
	/// </summary>
	Control clueModalContainer = new();
	
	/// <summary>
	/// The GridContainer modal storing information about a clue.
	/// </summary>
	GridContainer clueModal = new();

	/// <summary>
	/// The List of Note objects the player stores. Can be checked using a keyboard command and read.
	/// </summary>
	List<Note> notes = new();

	/// <summary>
	/// The List of Clue objects the player stores.
	/// </summary>
	List<Clue> clues = new();

	/// <summary>
	/// The List of Mission objects the player stores.
	/// </summary>
	List<Mission> missions = new();

	List<RichTextLabel> missionLabels = new();

	public bool newClue = false;

	bool clueModalOpen = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tabContainer = GetNodeOrNull<TabContainer>("TabContainer");
		tabContainer.SetTabTitle(0, "Suspects");
		tabContainer.SetTabTitle(1, "Saved Notes");
		tabContainer.SetTabTitle(2, "Victim");
		tabContainer.SetTabTitle(3, "Tasks");
		tabContainer.SetTabTitle(4, "Clues");
		suspectWindow = GetNodeOrNull<Control>("TabContainer/SuspectWindow");
		suspectDescription = GetNodeOrNull<RichTextLabel>("TabContainer/SuspectWindow/Description");
		noteWindow = GetNodeOrNull<ScrollContainer>("TabContainer/NoteWindow");
		savedDialogueContainer = GetNodeOrNull<VBoxContainer>("TabContainer/NoteWindow/MarginContainer/SavedNotes");
		suspectDescription.Text = "";
		victimWindow = GetNodeOrNull<Control>("TabContainer/VictimWindow");
		victimWindow.GetNode<RichTextLabel>("Description").Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/MakotoBourdain.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
		missionWindow = GetNodeOrNull<ScrollContainer>("TabContainer/MissionWindow");
		missionContainer = GetNodeOrNull<VBoxContainer>("TabContainer/MissionWindow/MarginContainer/MissionList");
		clueWindow = GetNodeOrNull<Control>("TabContainer/ClueWindow");
		clueContainer = GetNode<GridContainer>("TabContainer/ClueWindow/GridContainer");
		clueModalContainer = GetNode<Control>("TabContainer/ClueWindow/SelectedClue");
		clueModalContainer.GuiInput += @event => CloseClue(@event);
		clueModalContainer.Visible = false;
		clueModal = GetNode<GridContainer>("TabContainer/ClueWindow/SelectedClue/Modal");

		try {
			SetNotes(JsonSerializer.Deserialize<List<Note>>(
				Godot.FileAccess.Open(Globals.pathToNotes, Godot.FileAccess.ModeFlags.Read).GetAsText(),
				Globals.options
			));
		} catch (JsonException) {
			// JsonSerialiser.Deserialise() throws an exception if there's nothing in the target file to deserialize. 
			// This catches and ignores the exception.
			GD.Print("No saved notes from last load.");
		}

		try {
			SetMissions(PlayerVariables.GetNextMissions());
		} catch (JsonException) {
			// JsonSerialiser.Deserialise() throws an exception if there's nothing in the target file to deserialize. 
			// This catches and ignores the exception.
			GD.Print("No missions from last load.");
		}
		
		try {
			SetClues(PlayerVariables.GetFoundClues());
		} catch (JsonException) {
			// JsonSerialiser.Deserialise() throws an exception if there's nothing in the target file to deserialize. 
			// This catches and ignores the exception.
			GD.Print("No clues from last load.");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (Visible) {
			if (@event.IsActionPressed("ui_left")) {
				tabContainer.CurrentTab = Mathf.PosMod(tabContainer.CurrentTab - 1, tabContainer.GetTabCount());
			}
			if (@event.IsActionPressed("ui_right")) {
				tabContainer.CurrentTab = (tabContainer.CurrentTab + 1) % tabContainer.GetTabCount();
			}
		}
	}



	public void SetNotes(List<Note> nns) {
		notes = nns;
		foreach (Note n in nns) {
			RichTextLabel rtl = new()
			{
				BbcodeEnabled = true,
				ScrollActive = false,
				FitContent = true,
				DeselectOnFocusLossEnabled = false
			};

			rtl.AddThemeFontOverride("normal_font", GameManager.normalFont);
			rtl.AddThemeFontOverride("bold_font", GameManager.boldFont);

			rtl.Text = $"[b][i]{n.NPCName}, {Globals.dayToDate[n.Day]}[/i][/b]\n\n{n.Text}";
			savedDialogueContainer.AddChild(rtl);
			rtl.GuiInput += @event => RemoveNote(@event, rtl, n);

			HSeparator hs1 = new();
			savedDialogueContainer.AddChild(hs1);
		}
	}

	/// <summary>
	/// Store dialogue, the day it was spoken, and who spoke it into the Player's notebook as a Note. Will not store duplicate notes.
	/// </summary>
	/// <param name="txt"></param>
	/// <param name="day"></param>
	/// <param name="npc"></param>
	public void SaveDialogueAsNote(string txt, int day, string npcName) {
		Note nn = new()
		{
			Text = txt,
			Day = day,
			NPCName = npcName
		};

		if (!notes.Contains(nn)) {
			notes.Add(nn);

			string newNote = JsonSerializer.Serialize(notes, Globals.options);
			File.WriteAllText(Globals.pathToNotes, newNote);

			RichTextLabel rtl = new()
			{
				BbcodeEnabled = true,
				ScrollActive = false,
				FitContent = true,
				SelectionEnabled = true
			};

			rtl.AddThemeFontOverride("normal_font", GameManager.normalFont);
			rtl.AddThemeFontOverride("bold_font", GameManager.boldFont);

			rtl.Text = $"[b][i]{npcName}, {Globals.dayToDate[day]}[/i][/b]\n\n{txt}";
			savedDialogueContainer.AddChild(rtl);
			rtl.GuiInput += @event => RemoveNote(@event, rtl, nn);

			HSeparator hs1 = new();
			savedDialogueContainer.AddChild(hs1);
			noteWindow.ScrollVertical = (int)noteWindow.GetVScrollBar().MaxValue;
		}
	}

	/// <summary>
	/// Remove a note from the Notebook. This requires a right-click to work.
	/// </summary>
	/// <param name="event"></param>
	/// <param name="pos"></param>
	void RemoveNote(InputEvent @event, RichTextLabel rtl, Note n) {
		if (@event is InputEventMouseButton mb) {
			if (mb.ButtonMask == MouseButtonMask.Right && mb.Pressed) {
				var chs = savedDialogueContainer.GetChildren();
				int tbd = chs.IndexOf(rtl);
				chs[tbd].QueueFree();
				chs[tbd + 1].QueueFree();
				notes.Remove(n);

				string newNote = JsonSerializer.Serialize(notes, Globals.options);
				File.WriteAllText(Globals.pathToNotes, newNote);
			}
		}
	}

	public void SetMissions(List<Mission> mms) {
		// missions = mms;
		foreach (Mission m in mms) {
			AddMission(m);
		}
	}

	/// <summary>
	/// Add a mission to the Notebook.
	/// </summary>
	/// <param name="m"></param>
	public void AddMission(Mission m) {
		if (!missions.Contains(m)) {
			missions.Add(m);

			RichTextLabel rtl = new()
			{
				BbcodeEnabled = true,
				ScrollActive = false,
				FitContent = true,
				DeselectOnFocusLossEnabled = false
			};

			rtl.AddThemeFontOverride("normal_font", GameManager.normalFont);
			rtl.AddThemeFontOverride("bold_font", GameManager.boldFont);

			rtl.Text = m.Completed ?
				$"[fgcolor=#00000088][i]{m.Name}[/i][/fgcolor]\n\n[fgcolor=#00000088]{m.Description}[/fgcolor]" :
				$"[b][i]{m.Name}[/i][/b]\n\n{m.Description}"
			;
			missionContainer.AddChild(rtl);
			rtl.GuiInput += @event => RemoveMission(@event, rtl, m);
			missionLabels.Add(rtl);
			HSeparator hs1 = new();
			missionContainer.AddChild(hs1);
		}
	}

	/// <summary>
	/// Mark a mission as completed.
	/// </summary>
	/// <param name="m"></param>
	public void CompleteMission(Mission m) {
		RichTextLabel rtl = missionLabels[missions.IndexOf(m)];
		rtl.Text = $"[fgcolor=#00000088][i]{m.Name}[/i][/fgcolor]\n\n[fgcolor=#00000088]{m.Description}[/fgcolor]";
	}

	/// <summary>
	/// Remove a mission from the Notebook when it is completed.
	/// </summary>
	/// <param name="event"></param>
	/// <param name="pos"></param>
	void RemoveMission(InputEvent @event, RichTextLabel rtl, Mission m) {
		if (@event is InputEventMouseButton mb) {
			if (mb.ButtonMask == MouseButtonMask.Right && mb.Pressed) {
				if (m.Completed) {
					m.Deactivate();
					var chs = missionContainer.GetChildren();
					int tbd = chs.IndexOf(rtl);
					chs[tbd].QueueFree();
					chs[tbd + 1].QueueFree();
					missions.Remove(m);
				}
			}
		}
	}

	public void SetClues(List<Clue> cs) {
		foreach (Clue c in cs) {
			AddClue(c, false);
		}
	}

	public void AddClue(Clue c, bool showNewClue = true) {
		if (!clues.Contains(c)) {
			clues.Add(c);
			c.Found = true;
			newClue = showNewClue;

			BaseButton button;
			if (c.Texture is null) {
				button = new Button
				{
					Text = $"{c.Title}",
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					SizeFlagsVertical = SizeFlags.ExpandFill,
					Theme = GameManager.buttonTheme
				};
			} else {
				button = new TextureButton
				{
					IgnoreTextureSize = true,
					StretchMode = TextureButton.StretchModeEnum.Scale,
					TextureNormal = GD.Load<Texture2D>(Globals.resPathToShowcases + c.Texture),
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
					SizeFlagsVertical = SizeFlags.ExpandFill
				};
			}
			button.Pressed += () => OpenClue(c);
			clueContainer.AddChild(button);
			
		}
	}

	public void OpenClue(Clue c) {
		clueModalOpen = true;
		clueModalContainer.Visible = true;
		clueModal.GetNode<RichTextLabel>("RichTextLabel").Text = $"\n[center][b][font_size=40]{c.Title}[/font_size][/b]\n\n\n{c.Description}\n";
		if (c.Texture is not null) {
			clueModal.Columns = 2;
			clueModal.GetNode<TextureRect>("TextureRect").Visible = true;
			clueModal.GetNode<TextureRect>("TextureRect").Texture = GD.Load<Texture2D>(Globals.resPathToShowcases + c.Texture);
		} else {
			clueModal.Columns = 1;
			clueModal.GetNode<TextureRect>("TextureRect").Visible = false;
		}
	}

	/// <summary>
	/// Remove a mission from the Notebook when it is completed.
	/// </summary>
	/// <param name="event"></param>
	/// <param name="pos"></param>
	void CloseClue(InputEvent @event) {
		if (@event is InputEventMouseButton mb) {
			if (mb.ButtonMask == MouseButtonMask.Left && mb.Pressed) {
				if (clueModalOpen) {
					clueModalOpen = false;
					clueModalContainer.Visible = false;
				}
			}
		}
	}

	public void _on_tab_container_tab_button_pressed(int tab) {
        GD.Print(tab);
        if (tab != 0) suspectDescription.Text = "";
	}

	void _on_tab_container_tab_changed(int tab) {
		AudioManager.PlayOnce("res://Assets/Audio/Other/page_turn.wav");
	}

	public void _on_pc_button_pressed() {
		// patrick cuno
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/PatrickCuno.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
	public void _on_jl_button_pressed() {
		// johnny lawn
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/JohnnyLawn.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
	public void _on_ip_button_pressed() {
		// ignacio pascal
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/IgnacioPascal.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
	public void _on_aa_button_pressed() {
		// adora aga
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/AdoraAga.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
	public void _on_jc_button_pressed() {
		// jordan cuno
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/JordanCuno.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
	public void _on_fs_button_pressed() {
		// farah sal-cuno
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/FarahSalCuno.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
	public void _on_mb_button_pressed() {
		// marie bornstein
		suspectDescription.Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/MarieBornstein.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();
	}
}

/// <summary>
/// The Note class. Contains information about the text, day, and NPC of a section of spoken dialogue.
/// </summary>
public partial class Note : IEquatable<Note> {
	/// <summary>
	/// The text (dialogue) written in the note.
	/// </summary>
	[JsonPropertyName("Text")]
	public string Text { set; get; }

	/// <summary>
	/// The day the note was written for.
	/// </summary>
	[JsonPropertyName("Day")]
	public int Day { set; get; }

	/// <summary>
	/// The NPC who said the dialogue.
	/// </summary>
	[JsonPropertyName("NPCName")]
	public string NPCName { set; get; }

	public bool Equals(Note n) {
		return Text == n.Text &&
				Day == n.Day &&
				NPCName == n.NPCName;
	}
	public override int GetHashCode() {
		return HashCode.Combine(Text, Day, NPCName);
	}
}

/// <summary>
/// The Clue class. Contains information about clues that will be displayed in the Clue Window of the Notebook.
/// </summary>
public partial class Clue : IEquatable<Clue> {
	[JsonPropertyName("Title")]
	public string Title { set; get; }
	
	[JsonPropertyName("Shorthand")]
	public string Shorthand { set; get; }
	
	[JsonPropertyName("Description")]
	public string Description { set; get; }
	
	[JsonPropertyName("Texture")]
	public string Texture { set; get; }
	
	[JsonPropertyName("Found")]
	public bool Found { set; get; }

	public bool Equals(Clue n) {
		return Title == n.Title &&
				Shorthand == n.Shorthand &&
				Description == n.Description &&
				Texture == n.Texture && 
				Found == n.Found;
	}
	public override int GetHashCode() {
		return HashCode.Combine(Title, Shorthand, Description, Texture, Found);
	}

}
