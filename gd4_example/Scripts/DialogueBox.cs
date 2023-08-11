using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class DialogueBox : Control
{
	// public RichTextLabel missionText = new();
	public RichTextLabel txt = new();
	public RichTextLabel chcs = new();
	public Control choiceControl = new();
	public TextureRect txtbg = new();
	public TextureRect chcsbg = new();
	public TextureRect finishedMarker = new();
	public DialogueObject dialogue = new();
	public Array<Button> choiceButtons = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(GetParent().Name);
		// gui = (GUI)GetParent();
		txt = (RichTextLabel)GetNode("TxtLabel");
		txtbg = (TextureRect)GetNode("DBBG");
		finishedMarker = (TextureRect)GetNode("FinishedMarker");
		choiceControl = (Control)GetNode("ChoiceControl");
		chcs = (RichTextLabel)GetNode("ChoiceControl/ChoiceLabel");
		chcsbg = (TextureRect)GetNode("ChoiceControl/CBBG");
		txt.Text = "";
		chcs.Text = "";
		txt.VisibleRatio = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		txtbg.Visible = txt.Visible;
		chcsbg.Visible = chcs.Visible;
	}

	public void LoadDialogue(DialogueObject d) {
		dialogue = d;
	}

	// Write custom dialogue and user choices in the dialogue box
	public void Write(string d) {
		WriteDialogue(d);
		WriteChoices();
	}

	// Write dialogue and user choices in the dialogue box
	public void Write() {
		WriteDialogue();
		WriteChoices();
	}

	// Write the dialogue in the dialogue object this dialogue box has loaded.
	public void WriteDialogue() {
		txt.Text = string.Join('\n', dialogue.dialogue);
		txt.VisibleCharacters = 0;
		DialogueManager.SetDialogueToUpdate(this, 3);
	}
	
	// Write custom dialogue in the dialogue box
	public void WriteDialogue(string d) {
		txt.Text = string.Join('\n', d);
		txt.VisibleCharacters = 0;
		DialogueManager.SetDialogueToUpdate(this, 3);
	}
	
	public void WriteChoices() {
		choiceControl.Visible = true;
		foreach (var b in choiceButtons) {
			b.QueueFree();
		}
		choiceButtons.Clear();

		var nbpos = new Vector2(0, 0);
		for (int i = 0; i < dialogue.choices.Count; i++) {
			GetParent<GUI>().canProgressDialogue = false;
			string c = dialogue.choices[i];
            Button nb = new() {
                Text = c,
                Position = nbpos
            };
			// some lambda thing. 
			// https://ask.godotengine.org/147861/there-way-provide-additional-arguments-for-pressed-event-c%23
			nb.Pressed += () => GetResponse(c); 
            nbpos += new Vector2(0, 32); // move each successive button down 32 pixels
			choiceButtons.Add(nb);
            choiceControl.AddChild(nb);
		}
		// chcs.Text = string.Join('\n', dialogue.choices);
		chcs.Visible = dialogue.choices.Count > 0;
	}

	// Get the response from a choice `s` and run any functions the choice had if they exist.
	public void GetResponse(Variant s) {
		GetParent<GUI>().canProgressDialogue = true;
		dialogue.CallChoiceFunctions(dialogue.choices.IndexOf((string)s));
		if (dialogue.parseResult is not null) {
			/* 
				If the dialogue scene was told to switch dialogues, update all dialogue tools
				parseResult[0] = List<DialogueObject> -> the list of dialogue objects to load in
				parseResult[1] = int -> the dialogue number to start the above list from
			 */

			GetParent<GUI>().talkingNPC.LoadDialogue((List<DialogueObject>)dialogue.parseResult[0]); // update dialogue scene
			GetParent<GUI>().talkingNPC.SetDialogueNumber((int)dialogue.parseResult[1]); // Start the dialogue at the specified number
			LoadDialogue(((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[1]]); // update this dialogue from the dialogue number (MUST BE LAST TO AVOID DESTORYING CURRENT DIALOGUE)
			WriteDialogue();
		}
		if (dialogue.functionResult is not null) GD.Print(dialogue.functionResult);
		// GD.Print(dialogue.responses[(int)s]);
		// if (dialogue.choiceResponses.ContainsKey((string)s)) GD.Print(dialogue.choiceResponses[(string)s]);
		if (dialogue.choiceResponses.ContainsKey((string)s)) {
			WriteDialogue(dialogue.choiceResponses[(string)s]);
			choiceControl.Visible = false;
		} else {
			// progress to next dialogue object if the dialogue progresses any further
			if (!dialogue.choiceFunctions.Contains("EndDialogueB")) GetParent<GUI>().ProgressDialogue(GetParent<GUI>().talkingNPC); 
		}
	}
}
