using Godot;
using System.Collections.Generic;

public partial class NPC : StaticBody2D
{
	public List<string> missions = new();
	public List<DialogueObject> dialogue = new();
	public string diagPath;
	public int currentDiag = 0;
	public string secondaryDiagPath = null;
	public int secondaryDiagStart = 0;
	public bool isTalking = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadDialogue(diagPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddMission(string parameter) {
		missions.Add(parameter);
	}

	/// <summary>
	/// Load the DialogueObjects for this NPC from the list of DialogueObjects <c>d</c>. By default, the NPC's dialogue number is set to 0.
	/// </summary>
	/// <param name="d"></param>
	public void LoadDialogue(List<DialogueObject> d, int diagStart = 0) {
		dialogue = d;
		diagPath = d[0].originFilePath; // pick any dialogue object and get its dialogue file
		currentDiag = diagStart;
	}
	
	/// <summary>
	/// Load the DialogueObjects for this NPC from the dialogue file at <c>strPath</c>. By default, the NPC's dialogue number is set to 0.
	/// </summary>
	/// <param name="strPath"></param>
	/// <param name="diagStart"></param>
	public void LoadDialogue(string strPath, int diagStart = 0) {
		dialogue = DialogueManager.Parse(strPath);
		diagPath = strPath;
		currentDiag = diagStart;
	}

	/// <summary>
	/// Set the dialogue that will be used when this NPC has finished their current dialogue.
	/// </summary>
	/// <param name="sp"></param>
	public void SetSecondaryDialogue(string sp, int sds = 0) {
		secondaryDiagPath = sp;
		secondaryDiagStart = sds;
	}

	/// <summary>
	/// Load the dialogue to be used when the primary dialogue has finished.
	/// </summary>
	public void LoadSecondaryDialogue(int diagNum = -1) {
		if (diagNum == -1) diagNum = currentDiag;
		if (secondaryDiagPath != null) {
		// if (secondaryDiagPath != null && isTalking == false) {
		// if (secondaryDiagPath != null && diagNum == dialogue.Count) {
			LoadDialogue(secondaryDiagPath, secondaryDiagStart);
			GD.Print(currentDiag);
			secondaryDiagPath = null;
			secondaryDiagStart = 0;
		}
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
		if (currentDiag < dialogue.Count) {
			LoadSecondaryDialogue();
			var d = dialogue[currentDiag++];
			isTalking = true;
			return d;
		} else {
			RestartDialogue();
			isTalking = false;
			return null;
		}
	}

	/// <summary>
	/// Set the dialogue number for this NPC's dialogue to <c>n</c>.
	/// </summary>
	/// <param name="n"></param>
	public void SetDialogueNumber(int n) {
		currentDiag = n;
	}

	/// <summary>
	/// Restart the current dialogue scene.
	/// </summary>
	public void RestartDialogue() {
		currentDiag = 0;
	}
	
	/// <summary>
	/// Reset the dialogue to a specified dialogue scene
	/// </summary>
	/// <param name="str_path"></param>
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
