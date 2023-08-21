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
	Control suspectWindow = new();
	RichTextLabel suspectDescription = new();

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
		suspectWindow = GetNodeOrNull<Control>("TabContainer/SuspectWindow");
		suspectDescription = GetNodeOrNull<RichTextLabel>("TabContainer/SuspectWindow/Description");
		savedDialogueContainer = GetNodeOrNull<VBoxContainer>("TabContainer/NoteWindow/SavedNotes");
		suspectDescription.Text = "";

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

			rtl.Text = $"[b][i]{n.NPCName}, {n.Year}[/i][/b]\n\n{n.Text}";
			savedDialogueContainer.AddChild(rtl);
			rtl.GuiInput += @event => RemoveNote(@event, rtl, n);

			HSeparator hs1 = new();
			savedDialogueContainer.AddChild(hs1);
		}
	}

	/// <summary>
	/// Store dialogue, the year it was spoken, and who spoke it into the Player's notebook as a Note. Will not store duplicate notes.
	/// </summary>
	/// <param name="txt"></param>
	/// <param name="year"></param>
	/// <param name="npc"></param>
	public void SaveDialogueAsNote(string txt, int year, string npcName) {
		Note nn = new()
		{
			Text = txt,
			Year = year,
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
				FitContent = true
			};

			rtl.AddThemeFontOverride("normal_font", GameManager.normalFont);
			rtl.AddThemeFontOverride("bold_font", GameManager.boldFont);

			rtl.Text = $"[b][i]{npcName}, {year}[/i][/b]\n\n{txt}";
			savedDialogueContainer.AddChild(rtl);
			rtl.GuiInput += @event => RemoveNote(@event, rtl, nn);

			HSeparator hs1 = new();
			savedDialogueContainer.AddChild(hs1);
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
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: White
[b]Hair[/b]: Blond
[b]D.O.B.[/b]: 1926/04/29
[b]Pronouns[/b]: He/him
[b]Description[/b]: Patrick Cuno is the mayor of Shune, a small town on the outskirts of Dublin. He was voted into power nearly a decade ago, and everyone regrets their decision. Cuno's awful ruling has caused poverty, destruction, and corruption to plague the town, and yet he keeps getting voted into power by people who fear change. 

A joke entry by an unknown person resulted in Makoto Bourdain being entered into the upcoming election, and he received 48% of all votes without campaigning. He reportedly didn't even know he was running. Cuno took a very bold stance on the matter, pulling out show after show to get the public back on his side. However, nothing he did was working, and he seemed set to lose to Makoto in the next election until his sudden death. The results showed Makoto was leading at 64% to Cuno's 33%. The date of the election was September 10th.

It should be noted that Cuno's then-wife, Alice Cuno, divorced him on October 1st. Rumours of his affairs seemed to spread everywhere, but the most prevalent of them all was a picture taken of what looked like Cuno and Marie Bornstein, a cashier, walking out of a hotel together.
";
	}
	public void _on_jl_button_pressed() {
		// johnny lawn
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: White
[b]Hair[/b]: Blond
[b]D.O.B.[/b]: 1934/03/11
[b]Pronouns[/b]: He/him
[b]Description[/b]: Johnny Lawn was a close friend of Makoto. Makoto was frequently bullied due to his skinny stature. Johnny, whose father was a police officer, took it upon himself to protect him, though he still sometimes asked for bits of Makoto's lunch in return. The two grew up in a small community, going to the same primary and secondary schools.

After Makoto went abroad for his doctorate, Johnny began to miss the feelings of power he got. He thought becoming a police officer would satisfy that itch, and it did to an extent, but actual power, [i]real power[/i] had left his grasp along with Makoto. Upon his arrival, his expertise was widely sought after and he made a tremendous amount of money. Johnny saw this as Makoto upstaging him, and his resentment only grew.
";
	}
	public void _on_ip_button_pressed() {
		// ignacio pascal
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: Latino
[b]Hair[/b]: White
[b]D.O.B.[/b]: 1922/11/29
[b]Pronouns[/b]: He/him
[b]Description[/b]: 
";
	}
	public void _on_aa_button_pressed() {
		// adora aga
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: Black
[b]Hair[/b]: Black
[b]D.O.B.[/b]: 1952/08/08
[b]Pronouns[/b]: She/her
[b]Description[/b]: The wife of the deceased Makoto Bourdain. They met while Makoto returned to his father's village in France to get his doctorate there and promptly began dating. They married a few years later and returned from France with plans to have children. Makoto's job prospects were massive, so Adora had no need to find a job and prepared to become a stay at home mom. However, Makoto ended up changing his mind about having children, seemingly on a whim. Adora resented him for this but stayed by his side regardless, just in case he changed his mind.

She had called several lawyers to initiate divorce proceedings, but stopped short every time. The last time she called a divorce lawyer was on September 2nd, six days before Makoto's death.
";
	}
	public void _on_jc_button_pressed() {
		// jordan cuno
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: Black/white (mixed)
[b]Hair[/b]: Brown
[b]D.O.B.[/b]: 1963/01/05
[b]Pronouns[/b]: They/them
[b]Description[/b]: 
";		
	}
	public void _on_fs_button_pressed() {
		// farah sal-cuno
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: Brown (Indian)
[b]Hair[/b]: Black
[b]D.O.B.[/b]: 1961/15/12
[b]Pronouns[/b]: She/her
[b]Description[/b]: 
";
	}
	public void _on_mb_button_pressed() {
		// marie bornstein
		suspectDescription.Text = @"[b]Skin/Ethnicity[/b]: Jewish
[b]Hair[/b]: Black
[b]D.O.B.[/b]: 1947/19/06
[b]Pronouns[/b]: She/her
[b]Description[/b]: 
";
	}
}

/// <summary>
/// The Note class. Contains information about the text, year, and NPC of a section of spoken dialogue.
/// </summary>
public partial class Note : IEquatable<Note> {
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

	public bool Equals(Note n) {
		return Text == n.Text &&
				Year == n.Year &&
				NPCName == n.NPCName;
	}

	public override int GetHashCode() {
		return HashCode.Combine(Text, Year, NPCName);
	}
}
