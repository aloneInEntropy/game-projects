using Godot;
using System.Collections.Generic;

public partial class NPC : StaticBody2D
{
	public List<DialogueObject> dialogue = new();
	public int currentDiag = 0;
	protected string diagPath;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadDialogue(diagPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LoadDialogue(List<DialogueObject> d) {
		dialogue = d;
	}
	
	public void LoadDialogue(string str_path) {
		// diagPath = str_path;
		dialogue = DialogueManager.Parse(str_path);
		currentDiag = 0;
	}
	
	public void AddDialogue(DialogueObject d) {
		dialogue.Add(d);
	}
	
	public void RemoveDialogue(DialogueObject d) {
		dialogue.Remove(d);
	}

	// Get the next line of dialogue from this NPC. If there are no more lines of dialogue, return null.
	public DialogueObject GetNextDialogue() {
		// if dialogue has finished being typed out
		// currentDiag = Mathf.Clamp(currentDiag, 0, dialogue.Count);
		if (currentDiag < dialogue.Count) {
			var d = dialogue[currentDiag++];
			d.CallDialogueFunctions(0);
			return d;
		} else {
			RestartDialogue();
			return null;
		}
	}

	// Set the dialogue number for this NPC's dialogue
	public void SetDialogueNumber(int n) {
		currentDiag = n;
	}

	// Restart the current dialogue scene
	public void RestartDialogue() {
		currentDiag = 0;
	}
	
	// Reset the dialogue to the original dialogue scene
	public void ResetDialogue(string str_path) {
		LoadDialogue(str_path);
		currentDiag = 0;
	}



	void _on_interact_box_area_entered(Area2D area) {
		// GD.Print(area.GetType().Name);
		if (area.GetParent().GetType() == typeof(Player)) {
			// GD.Print(string.Format("{0} entered {1}", area.GetParent().Name, Name));
		}
	}

	void _on_interact_box_area_exited(Area2D area) {
		// GD.Print(area.GetType().Name);
		if (area.GetParent().GetType() == typeof(Player)) {
			RestartDialogue();
			LoadDialogue(diagPath);
			// if (overlapping.Contains(body)) overlapping.Remove(body);
			// GD.Print(String.Format("{0} entered {1}", Name, body.Name));
		}
	}
	void _on_interact_box_body_entered(Node2D body) {
		// GD.Print(body.GetType().Name);
		if (body.GetType().Name == "Player") {
			// GD.Print(String.Format("{0} entered {1}", Name, body.Name));
		}
	}

	void _on_interact_box_body_exited(Node2D body) {
		// GD.Print(body.GetType().Name);
		if (body.GetType().Name == "Player") {
			// if (overlapping.Contains(body)) overlapping.Remove(body);
			// GD.Print(String.Format("{0} entered {1}", Name, body.Name));
		}
	}
}
