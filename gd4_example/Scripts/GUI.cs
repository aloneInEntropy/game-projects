using Godot;
using System.Collections.Generic;

public partial class GUI : CanvasLayer
{
	public RichTextLabel missionText = new();
	public DialogueBox db = new();

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
		// dialogue_box
		db = (DialogueBox)GetNode("DialogueBox");
		db.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		missionText.Text = "";
		if (isDialogueActive) {
			foreach (var m in talkingNPC.GetUncompletedMissions()) {
				missionText.Text += m.Name + "\n";
			}
		}
	}

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
		if (!IsInstanceValid(db)) {
			db = dialogue_box.Instantiate<DialogueBox>();
			AddChild(db);
			db.Owner = this; // instanced nodes don't have an scene root, so add this GUI as its parent
		} 
		db.LoadDialogue(dialogue);
	}

	public void OpenDialogue() {
		isDialogueActive = true;
		db.Visible = true;
		db.Write();
	}

	public void CloseDialogue() {
		if (IsInstanceValid(db)) {
			db.QueueFree();
			db = null;
		}
		if (IsInstanceValid(talkingNPC)) {
			talkingNPC.ResetDialogue(talkingNPC.diagPath);
			talkingNPC = null;
		}
		isDialogueActive = false;
	}
}
