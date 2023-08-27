using Godot;
using Godot.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;

public partial class DialogueBox : Control
{
	/// <summary>
	/// The RichTextLabel the dialogue box uses to display text.
	/// </summary>
	public RichTextLabel txt = new();

	/// <summary>
	/// The RichTextLabel used to display the name of the presently-speaking NPC.
	/// </summary>
	public RichTextLabel nameLabel = new();

	public VBoxContainer choiceControl = new();
	public TextureRect txtbg = new();
	public TextureRect finishedMarker = new();

	/// <summary>
	/// The DialogueObject currently in the dialogue box.
	/// </summary>
	public DialogueObject dialogue = new();

	/// <summary>
	/// The Array of Buttons used for choices.
	/// </summary>
	public Array<Button> choiceButtons = new();

	/// <summary>
	/// The TextureRect used to display the portrait of the presently-speaking NPC.
	/// </summary>
	public TextureRect portrait = new();

	/// <summary>
	/// Is the dialogue box displayed?
	/// </summary>
	public bool isOpen = false;

	/// <summary>
	/// Are the buttons ready to be clicked?
	/// </summary>
	private bool buttonsReady = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// GD.Print(GetParent().Name);
		// gui = (GUI)GetParent();
		txt = (RichTextLabel)GetNode("TxtLabel");
		nameLabel = (RichTextLabel)GetNode("NameLabel");
		txtbg = (TextureRect)GetNode("DBBG");
		finishedMarker = (TextureRect)GetNode("FinishedMarker");
		choiceControl = (VBoxContainer)GetNode("ChoiceControl");
		portrait = (TextureRect)GetNode("Portrait");
		txt.Text = "";
		txt.VisibleRatio = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		txtbg.Visible = txt.Visible;
		if (!DialogueManager.isDialogueReading) {
			if (choiceButtons.Count > 0 && buttonsReady) {
				choiceButtons[0].GrabFocus();
				buttonsReady = false;
			} 
		}
	}

	/// <summary>
	/// Load the DialogueObject for the dialogue box
	/// </summary>
	/// <param name="d"></param>
	public void LoadDialogue(DialogueObject d) {
		dialogue = d;
		nameLabel.Text = Globals.talkingNPC.Name;
	}

	/// <summary>
	/// Write custom strings. For custom dialogue, use Write(string).
	/// </summary>
	/// <param name="d"></param>
	public void WriteCustom(string d, string nameLabelTitle = "Narrator", string voicePath = "null", string portraitPath = "null") {
		choiceControl.Visible = false;
		WriteDialogue(d);
		Modify(nameLabelTitle, voicePath, portraitPath);
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
		txt.Text = GameManager.RemoveBBCTags(string.Join('\n', dialogue.dialogue));
		txt.VisibleCharacters = 0;
		DialogueManager.SetDialogueToUpdate(this, frames_before_updating:2);
	}
	
	/// <summary>
	/// Write custom dialogue in the dialogue box
	/// </summary>
	/// <param name="d"></param>
	public void WriteDialogue(string d) {
		txt.Text = GameManager.RemoveBBCTags(d);
		txt.VisibleCharacters = 0;
		DialogueManager.SetDialogueToUpdate(this, frames_before_updating:2);
	}
	
	/// <summary>
	/// Display the choices for the current DialogueObject if there are any and ready them for usage.
	/// </summary>
	public void WriteChoices() {
		choiceControl.Visible = true;
		foreach (var b in choiceButtons) {
			b.QueueFree();
		}
		choiceButtons.Clear();

		for (int i = 0; i < dialogue.choices.Count; i++) {
			GetParent<GUI>().canProgressDialogue = false;
			string c = dialogue.choices[i];
            Button nb = new() {
                Text = c,
				Theme = GD.Load<Theme>("res://Resources/ButtonTheme.tres"),
				FocusMode = FocusModeEnum.All
            };
			// some lambda thing. 
			// https://ask.godotengine.org/147861/there-way-provide-additional-arguments-for-pressed-event-c%23
			nb.Pressed += () => DisplayChoiceResponse(c);
			// nb.Owner = GetParent().GetParent();
			choiceButtons.Add(nb);
            choiceControl.AddChild(nb);
		}
		buttonsReady = true;
	}

	/// <summary>
	/// Run all dialogue functions for the DialogueObject stored in this DialogueBox.
	/// </summary>
	public void DisplayDialogueResult() {
		// If dialogue file set to end, IMMEDIATELY DISCARD DIALOGUE FILE to prevent dialogue cancel loop.
		// Since this will end the dialogue, do NOT save the previous dialogue position and restart it instead.
		if (dialogue.originFilePath == "end.txt") {
			Globals.talkingNPC.LoadWaitingDialogue(0);
		}

		dialogue.CallDialogueFunctions(); // Call all dialogue functions for this DialogueObject
		if (dialogue.parseResult is not null && dialogue.parseResult.Count != 0) {
			/* 
				If the dialogue scene was told to switch dialogues, update all dialogue tools.

				parseResult[0] = List<DialogueObject> -> the list of dialogue objects to load in
				parseResult[1] = bool -> choose whether or not to immediately load parseResult[0]
				parseResult[2] = int -> the dialogue number to start the above list from
				parseResult[3] = bool -> choose whether or not to save parseResult[0] to the NPC
			 */
			if ((bool)dialogue.parseResult[1]) {
				// update dialogue scene
				Globals.talkingNPC.LoadDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2],
					(bool)dialogue.parseResult[3]
				); 
				// update this dialogue from the dialogue number (MUST BE LAST TO AVOID DESTORYING CURRENT DIALOGUE)
				LoadDialogue(((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]]); 
			} else {
				Globals.talkingNPC.SetSecondaryDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				);
			}
		}
		// if (dialogue.functionResult is not null) GD.Print(string.Format("DIALOGUE FUNCTION RESULT: {0}", dialogue.functionResult));
	}

	/// <summary>
	/// Get the response from a choice `s` and run any functions the choice had if they exist.
	/// </summary>
	public void DisplayChoiceResponse(Variant s) {
		GetParent<GUI>().canProgressDialogue = true; // Allow progression after making a choice.
		// If dialogue file set to end, IMMEDIATELY DISCARD DIALOGUE FILE to prevent dialogue cancel loop.
		// Since this will end the dialogue, do NOT save the previous dialogue position and restart it instead.
		if (dialogue.originFilePath == "end.txt") {
			Globals.talkingNPC.LoadWaitingDialogue(0);
		}

		// Call all functions for the choice at `s`
		dialogue.CallChoiceFunctions(dialogue.choices.IndexOf((string)s));

		if (dialogue.parseResult is not null && dialogue.parseResult.Count != 0) {
			/* 
				If the dialogue scene was told to switch dialogues, update all dialogue tools.

				parseResult[0] = List<DialogueObject> -> the list of dialogue objects to load in
				parseResult[1] = bool -> choose whether or not to immediately load parseResult[0]
				parseResult[2] = int -> the dialogue number to start the above list from
				parseResult[3] = bool -> choose whether or not to save parseResult[0] to the NPC
			 */
			if ((bool)dialogue.parseResult[1]) {
				// update dialogue scene
				Globals.talkingNPC.LoadDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2],
					(bool)dialogue.parseResult[3]
				); 
				// update this dialogue from the dialogue number (MUST BE LAST TO AVOID DESTORYING CURRENT DIALOGUE)
				LoadDialogue(((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]]); 
			} else {
				Globals.talkingNPC.SetSecondaryDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				);
			}
		}
		// if (dialogue.functionResult is not null) GD.Print(dialogue.functionResult); // choice function results (discarded for now)
		if (dialogue.choiceResponses.ContainsKey((string)s)) {
			WriteDialogue(dialogue.choiceResponses[(string)s]);
			DisplayResponseResult(dialogue.choiceResponses[(string)s]);
			choiceControl.Visible = false;
		} else {
			// progress to next dialogue object if the dialogue progresses any further
			if (!dialogue.choiceFunctions.Contains("EndDialogueB")) GetParent<GUI>().ProgressDialogue(Globals.talkingNPC); 
		}
	}

	/// <summary>
	/// Run all reponse functions for the DialogueObject stored in this DialogueBox.
	/// </summary>
	public void DisplayResponseResult(Variant s) {
		// If dialogue file set to end, IMMEDIATELY DISCARD DIALOGUE FILE to prevent dialogue cancel loop.
		// Since this will end the dialogue, do NOT save the previous dialogue position and restart it instead.
		if (dialogue.originFilePath == "end.txt") {
			Globals.talkingNPC.LoadWaitingDialogue(0);
		}

		dialogue.CallResponseFunctions(dialogue.responses.IndexOf((string)s)); // Call all dialogue functions for this DialogueObject
		if (dialogue.parseResult is not null && dialogue.parseResult.Count != 0) {
			/* 
				If the dialogue scene was told to switch dialogues, update all dialogue tools.

				parseResult[0] = List<DialogueObject> -> the list of dialogue objects to load in
				parseResult[1] = bool -> choose whether or not to immediately load parseResult[0]
				parseResult[2] = int -> the dialogue number to start the above list from
				parseResult[3] = bool -> choose whether or not to save parseResult[0] to the NPC
			 */
			if ((bool)dialogue.parseResult[1]) {
				// update dialogue scene
				Globals.talkingNPC.LoadDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2],
					(bool)dialogue.parseResult[3]
				); 
				// update this dialogue from the dialogue number (MUST BE LAST TO AVOID DESTORYING CURRENT DIALOGUE)
				LoadDialogue(((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]]); 
			} else {
				Globals.talkingNPC.SetSecondaryDialogue(
					((List<DialogueObject>)dialogue.parseResult[0])[(int)dialogue.parseResult[2]].originFilePath, 
					(int)dialogue.parseResult[2]
				);
			}
		}
		// if (dialogue.functionResult is not null) GD.Print(string.Format("DIALOGUE FUNCTION RESULT: {0}", dialogue.functionResult));
	}

	/// <summary>
	/// Modify the dialogue box and update any changes.
	/// </summary>
	/// <param name="nameTitle"></param>
	/// <param name="voicePath"></param>
	/// <param name="portraitPath"></param>
	public void Modify(string nameTitle, string voicePath = "Narrator.wav", string portraitPath = "null") {
		if (nameTitle == "Narrator") {
			nameLabel.Text = "";
			Globals.talkingNPC.SetVoice("Narrator.wav");
			portrait.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Blank.png");
		} else {
			nameLabel.Text = nameTitle;
			Globals.talkingNPC.SetVoice(Globals.GetNPC(nameTitle).voicePath);
			portrait.Texture = GD.Load<Texture2D>(Globals.resPathToPortraits + Globals.GetNPC(nameTitle).portraitPath);
		}
	}

	/// <summary>
	/// Open and show this dialogue box.
	/// </summary>
	public void Open() {
		isOpen = true;
		Visible = true;
	}
	
	/// <summary>
	/// Close and hide this dialogue box.
	/// </summary>
	public void Close() {
		isOpen = false;
		Visible = false;
	}
}
