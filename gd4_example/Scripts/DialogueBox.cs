using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class DialogueBox : Control
{
	// public RichTextLabel missionText = new();
	public RichTextLabel txt = new();
	public RichTextLabel chcs = new();
	public RichTextLabel nameLabel = new();
	public Control choiceControl = new();
	public TextureRect txtbg = new();
	public TextureRect chcsbg = new();
	public TextureRect finishedMarker = new();
	public DialogueObject dialogue = new();
	public Array<Button> choiceButtons = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// GD.Print(GetParent().Name);
		// gui = (GUI)GetParent();
		txt = (RichTextLabel)GetNode("TxtLabel");
		nameLabel = (RichTextLabel)GetNode("NameLabel");
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
		if (GetParent<GUI>().isDialogueActive) nameLabel.Text = GetParent<GUI>().talkingNPC.Name;
	}

	/// <summary>
	/// Load the DialogueObject for the dialogue box
	/// </summary>
	/// <param name="d"></param>
	public void LoadDialogue(DialogueObject d) {
		dialogue = d;
	}

	/// <summary>
	/// Write custom dialogue and user choices in the dialogue box
	/// </summary>
	/// <param name="d"></param>
	public void Write(string d) {
		WriteDialogue(d);
		WriteChoices();
	}

	/// <summary>
	/// Write dialogue and user choices in the dialogue box
	/// </summary>
	public void Write() {
		WriteDialogue();
		WriteChoices();
	}

	/// <summary>
	/// Write the dialogue in the dialogue object this dialogue box has loaded.
	/// </summary>
	public void WriteDialogue() {
		txt.Text = string.Join('\n', dialogue.dialogue);
		txt.VisibleCharacters = 0;
		DialogueManager.SetDialogueToUpdate(this, 3);
	}
	
	/// <summary>
	/// Write custom dialogue in the dialogue box
	/// </summary>
	/// <param name="d"></param>
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
			nb.Pressed += () => DisplayChoiceResponse(c); 
            nbpos += new Vector2(0, 32); // move each successive button down 32 pixels
			choiceButtons.Add(nb);
            choiceControl.AddChild(nb);
		}
		// chcs.Text = string.Join('\n', dialogue.choices);
		chcs.Visible = dialogue.choices.Count > 0;
	}

	/// <summary>
	/// Run all dialogue functions for the DialogueObject stored in this DialogueBox.
	/// </summary>
	public void DisplayDialogueResult() {
		dialogue.CallDialogueFunctions(); // Call all dialogue functions for this DialogueObject
		if (dialogue.parseResult is not null) {
			/* 
				If the dialogue scene was told to switch dialogues, update all dialogue tools
				parseResult[0] = List<DialogueObject> -> the list of dialogue objects to load in
				parseResult[1] = bool -> choose whether or not to immediately load parseResult[0]
				parseResult[2] = int -> the dialogue number to start the above list from
			 */
			// GD.Print((bool)dialogue.parseResult[1]);
			// GD.Print(dialogue.originFilePath);
			// var td = dialogue;
			// GetParent<GUI>().talkingNPC.LoadDialogue((List<DialogueObject>)dialogue.parseResult[0]); // update dialogue scene
			// GetParent<GUI>().talkingNPC.SetDialogueNumber((int)dialogue.parseResult[2]); // Start the dialogue at the specified number
			if ((bool)dialogue.parseResult[1]) {
				// update dialogue scene
				GetParent<GUI>().talkingNPC.LoadDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				); 
				// update this dialogue from the dialogue number (MUST BE LAST TO AVOID DESTORYING CURRENT DIALOGUE)
				LoadDialogue(((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]]); 
			} else {
				GetParent<GUI>().talkingNPC.SetSecondaryDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				);
			}
		}
		if (dialogue.functionResult is not null) GD.Print(string.Format("DIALOGUE FUNCTION RESULT: {0}", dialogue.functionResult));
	}

	/// <summary>
	/// Get the response from a choice `s` and run any functions the choice had if they exist.
	/// </summary>
	public void DisplayChoiceResponse(Variant s) {
		GetParent<GUI>().canProgressDialogue = true; // Allow progression after making a choice)
		dialogue.CallChoiceFunctions(dialogue.choices.IndexOf((string)s));
		if (dialogue.parseResult is not null) {
			if ((bool)dialogue.parseResult[1]) {
				// update dialogue scene
				GetParent<GUI>().talkingNPC.LoadDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				); 
				// update this dialogue from the dialogue number (MUST BE LAST TO AVOID DESTORYING CURRENT DIALOGUE)
				LoadDialogue(((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]]); 
			} else {
				GetParent<GUI>().talkingNPC.SetSecondaryDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				);
			}
		}
		if (dialogue.functionResult is not null) GD.Print(dialogue.functionResult); // choice function results (discarded for now)
		if (dialogue.choiceResponses.ContainsKey((string)s)) {
			WriteDialogue(dialogue.choiceResponses[(string)s]);
			choiceControl.Visible = false;
		} else {
			// progress to next dialogue object if the dialogue progresses any further
			if (!dialogue.choiceFunctions.Contains("EndDialogueB")) GetParent<GUI>().ProgressDialogue(GetParent<GUI>().talkingNPC); 
		}
	}
}
