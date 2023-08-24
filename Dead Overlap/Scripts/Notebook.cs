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
	TabContainer tabContainer = new();
	Control suspectWindow = new();
	Control victimWindow = new();
	ScrollContainer noteWindow = new();

	/// <summary>
	/// The RichTextLabel containing the description of the selected suspect.
	/// </summary>
	RichTextLabel suspectDescription = new();

	/// <summary>
	/// The VBoxContainer storing all saved notes as RichTextLabels and their respective HSeparators.
	/// </summary>
	VBoxContainer savedDialogueContainer = new();

    /// <summary>
    /// The List of Note objects the player stores. Can be checked using a keyboard command and read.
    /// </summary>
    List<Note> notes = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tabContainer = GetNodeOrNull<TabContainer>("TabContainer");
		tabContainer.SetTabTitle(0, "Suspects");
		tabContainer.SetTabTitle(1, "Saved Notes");
		tabContainer.SetTabTitle(2, "Victim");
		suspectWindow = GetNodeOrNull<Control>("TabContainer/SuspectWindow");
		suspectDescription = GetNodeOrNull<RichTextLabel>("TabContainer/SuspectWindow/Description");
		noteWindow = GetNodeOrNull<ScrollContainer>("TabContainer/NoteWindow");
		savedDialogueContainer = GetNodeOrNull<VBoxContainer>("TabContainer/NoteWindow/MarginContainer/SavedNotes");
		suspectDescription.Text = "";
		victimWindow = GetNodeOrNull<Control>("TabContainer/VictimWindow");
		victimWindow.GetNode<RichTextLabel>("Description").Text = Godot.FileAccess.Open("res://Assets/Text/Descriptions/MakotoBourdain.txt", Godot.FileAccess.ModeFlags.Read).GetAsText();


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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
				chs[tbd+1].QueueFree();
				notes.Remove(n);
            }
        }
	}

	public void _on_tab_container_tab_button_pressed(int tab) {
        GD.Print(tab);
        if (tab != 0) suspectDescription.Text = "";
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
