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
	VBoxContainer savedDialogueContainer = new();

    /// <summary>
    /// The notes the player stores. Can be checked using a keyboard command and read.
    /// </summary>
    List<Note> notes = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tabContainer = GetNodeOrNull<TabContainer>("TabContainer");
		tabContainer.SetTabTitle(0, "Suspects");
		tabContainer.SetTabTitle(1, "Saved Notes");
		savedDialogueContainer = GetNodeOrNull<VBoxContainer>("TabContainer/ScrollContainer/SavedNotes");
		try {
			SetNotes(JsonSerializer.Deserialize<List<Note>>(
				Godot.FileAccess.Open(Globals.pathToNotes, Godot.FileAccess.ModeFlags.Read).GetAsText(),
				Globals.options
			));
			
		}
		catch (JsonException) {
			GD.Print("No saved notes from last load.");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetNotes(List<Note> nns) {
		foreach (Note n in nns) {
			RichTextLabel rtl = new()
			{
				BbcodeEnabled = true,
				ScrollActive = false,
				FitContent = true
			};

			rtl.AddThemeFontOverride("normal_font", GameManager.normalFont);
			rtl.AddThemeFontOverride("bold_font", GameManager.boldFont);

			rtl.Text = $"[b][i]{n.NPCName}, {n.Year}[/i][/b]\n\n{n.Text}";
			savedDialogueContainer.AddChild(rtl);

			HSeparator hs1 = new();
			HSeparator hs2 = new();
			savedDialogueContainer.AddChild(hs1);
			savedDialogueContainer.AddChild(hs2);
		}
	}

	/// <summary>
	/// Store dialogue, the year it was spoken, and who spoke it into the Player's notebook as a Note. Will not store duplicate notes.
	/// </summary>
	/// <param name="txt"></param>
	/// <param name="year"></param>
	/// <param name="npc"></param>
	public void SaveDialogueAsNote(string txt, int year, NPC npc) {
		Note nn = new()
		{
			Text = txt,
			Year = year,
			NPCName = npc.trueName
		};

		if (!notes.Contains(nn)) {
			notes.Add(nn);
			
			string newNote = JsonSerializer.Serialize(notes, Globals.options);
			File.WriteAllText(Globals.pathToNotes, newNote);

			RichTextLabel rtl = new()
			{
				BbcodeEnabled = true,
				ScrollActive = false,
				FitContent = true
			};

			rtl.AddThemeFontOverride("normal_font", GameManager.normalFont);
			rtl.AddThemeFontOverride("bold_font", GameManager.boldFont);

			rtl.Text = $"[b][i]{npc.trueName}, {year}[/i][/b]\n\n{txt}";
			savedDialogueContainer.AddChild(rtl);

			HSeparator hs1 = new();
			HSeparator hs2 = new();
			savedDialogueContainer.AddChild(hs1);
			savedDialogueContainer.AddChild(hs2);
		}
	}
}

/// <summary>
/// The Note class. Contains information about the text, year, and NPC of a section of spoken dialogue.
/// </summary>
public partial class Note {
	/// <summary>
	/// The text (dialogue) written in the note.
	/// </summary>
	[JsonPropertyName("Text")]
	public string Text { set; get; }

	/// <summary>
	/// The year the note was written for.
	/// </summary>
	[JsonPropertyName("Year")]
	public int Year { set; get; }

	/// <summary>
	/// The NPC who said the dialogue.
	/// </summary>
	[JsonPropertyName("NPCName")]
	public string NPCName { set; get; }

	public override bool Equals(object n) {
		return Text.Equals(((Note)n).Text) &&
				Year.Equals(((Note)n).Year) &&
				NPCName.Equals(((Note)n).NPCName);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Text, Year, NPCName);
	}
}
