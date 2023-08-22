using Godot;


/// <summary>
/// The GUI class. Contains information and methods relating to the on-screen grephical user interface.
/// </summary>
public partial class GUI : CanvasLayer
{
	public RichTextLabel missionText = new();
	public DialogueBox db = new();
	public Notebook notebook = new();

	/// <summary>
	/// The NPC currently speaking.
	/// </summary>
	public NPC talkingNPC;

	/// <summary>
	/// Boolean for if the dialogue box is open.
	/// </summary>
	public bool isDialogueActive = false;

	/// <summary>
	/// Boolean checking if the player can progress dialogue using keyboard keys.
	/// </summary>
	public bool canProgressDialogue = true;
	private readonly PackedScene dialogue_box = GD.Load<PackedScene>("res://Scenes/DialogueBox.tscn");

	public override void _Ready()
	{
		Globals.gui = this;
		missionText = (RichTextLabel)GetNode("MissionText");
		db = (DialogueBox)GetNode("DialogueBox");
		db.Visible = false;
		notebook = (Notebook)GetNode("Notebook");
		notebook.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("pause")) {
			notebook.Visible = !notebook.Visible;
			GameManager.isGamePaused = !GameManager.isGamePaused;
		}

		if (isDialogueActive && IsInstanceValid(db) && @event.IsActionPressed("save_dialogue")) {
			// Don't save "un-spoken" dialogue.
			if (db.nameLabel.Text != "") notebook.SaveDialogueAsNote(db.txt.Text, Globals.year, db.nameLabel.Text);
		}
	}

	/// <summary>
	/// Displays and progresses the dialogue spoken by the NPC. If there is no more dialogue for the NPC to speak, the dialogue box is closed.
	/// </summary>
	/// <param name="npc"></param>
	public void ProgressDialogue(NPC npc) {
		talkingNPC = npc;
		if (canProgressDialogue) {
			DialogueObject d = npc.GetNextDialogue();
			if (d is null) {
				CloseDialogue();
			} else {
				SetDialogue(d);
				OpenDialogue();
				db.DisplayDialogueResult();
			}
		}
	}

	void SetDialogue(DialogueObject dialogue) {
		db.LoadDialogue(dialogue);
	}

	/// <summary>
	/// Open the dialogue box and write the string <c>d</c> into it. <br/>
	/// If <c>isDialogue</c> is true, assume there may be choices incoming. Otherwise, write string plainly.
	/// </summary>
	/// <param name="d"></param>
	/// <param name="isDialogue"></param>
	public void OpenDialogue(string d, bool isDialogue = true) {
		missionText.Text = "";
		isDialogueActive = true;
		db.Open();
		if (isDialogue) db.Write(d);
		else db.WriteCustom(d:d);
	}

	/// <summary>
	/// Open the dialogue box and write the current dialogue object into it. <br/>
	/// </summary>
	public void OpenDialogue() {
		missionText.Text = "";
		isDialogueActive = true;
		db.Open();
		db.Write();
	}

	public void CloseDialogue() {
		missionText.Text = "";
		db.Close();
		if (IsInstanceValid(talkingNPC)) {
			talkingNPC.ResetDialogue(talkingNPC.diagPath);
			talkingNPC = null;
		}
		isDialogueActive = false;
	}
}
