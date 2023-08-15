using Godot;
using System.Text.Json;
using System.Collections.Generic;
using System.Dynamic;
// using System.IO;



public partial class NPC : StaticBody2D
{
	/// <summary>
	/// The path to the voice sound file used by this NPC.
	/// </summary>
	public string voicePath = "";

	/// <summary>
	/// The voice used by this NPC.
	/// </summary>
	public AudioStreamWav voice = new();

	/// <summary>
	/// The path to the JSON file used to load missions for this NPC.
	/// </summary>
	public string missionJSONPath = "";

	/// <summary>
	/// The list of missions given to the Player by this NPC.
	/// </summary>
	public List<Mission> Missions { set;  get; }

	/// <summary>
	/// The list of dialogue spoken by this NPC.
	/// </summary>
	public List<DialogueObject> dialogue = new();

	[Export]
	/// <summary>
	/// Path to dialogue.
	/// </summary>
	public string diagPath;
	[Export]
	/// <summary>
	/// Dialogue section to display.
	/// </summary>
	public int currentDiag = 0;

	/// <summary>
	/// Path to dialogue to show when main dialogue has finished.
	/// </summary>
	public string secondaryDiagPath = null;
	public int secondaryDiagStart = 0; // point to start secondary dialogue from

	/// <summary>
	/// Path to store temporary dialogue in. Will be resumed when current dialogue is finished. Takes priority over secondary dialogue.
	/// </summary>
	public string tempDiagPath = null; 
	public int tempDiagStart = 0;

	/// <summary>
	/// Boolean checking if this NPC is talking (active in the dialogue box).
	/// </summary>
	public bool isTalking = false;
	

	/// <summary>
	/// Set the voice of this NPC from the path <c>vPath</c> to a .wav file.
	/// </summary>
	/// <param name="vPath"></param>
	public void SetVoice(string vPath) {
		voice = (AudioStreamWav)GD.Load(vPath);
	}

	/// <summary>
	/// Set the missions of the NPC using the path <c>missionPath</c> to a JSON file.
	/// </summary>
	/// <param name="missionPath"></param>
	public void SetMissionsJSON(string missionPath) {
		FileAccess file = FileAccess.Open(missionPath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		Missions = JsonSerializer.Deserialize<List<Mission>>(jsonString);
		file.Close();
	}

	/// <summary>
	/// Get a mission given its name <c>n</c>.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>The mission with the name <c>n</c>.</returns>
	public Mission GetMission(string n) {
		foreach (var m in Missions) {
			if (m.Name == n) return m;
		}
		return null;
	}
	
	/// <summary>
	/// Get a mission at position <c>n</c> in the stored list of missions.
	/// </summary>
	/// <param name="n"></param>
	/// <returns>The mission at position <c>n</c>.</returns>
	public Mission GetMission(int n) {
		return Missions[n];
	}
	
	/// <summary>
	/// Get all missions not yet completed.
	/// </summary>
	/// <returns>The list of missions that aren't completed.</returns>
	public List<Mission> GetUncompletedMissions() {
		List<Mission> ucm = new();
		foreach (var m in Missions) {
			if (!m.Completed) ucm.Add(m);
		}
		return ucm;
	}

	/// <summary>
	/// Load the DialogueObjects for this NPC from the list of DialogueObjects <c>d</c>. By default, the NPC's dialogue number is set to 0.
	/// </summary>
	/// <param name="d"></param>
	public void LoadDialogue(List<DialogueObject> d, int diagStart = 0, bool savePath = true) {
		if (!savePath) {
			SetTemporaryDialogue(diagPath, currentDiag);
		}
		dialogue = d;
		diagPath = d[0].originFilePath; // pick any dialogue object and get its dialogue file
		currentDiag = diagStart;
	}
	
	/// <summary>
	/// Load the DialogueObjects for this NPC from the dialogue file at <c>strPath</c>. <br/>
	/// By default, the NPC's dialogue number is set to 0 and the path given is saved into memory.
	/// </summary>
	/// <param name="strPath"></param>
	/// <param name="diagStart"></param>
	/// <param name="savePath"></param>
	public void LoadDialogue(string strPath, int diagStart = 0, bool savePath = true) {
		if (strPath[15..] != "end.txt") {
		}
		if (!savePath) {
			SetTemporaryDialogue(diagPath, currentDiag);
		}
		// GD.Print(strPath);
		dialogue = DialogueManager.Parse(strPath);
		diagPath = strPath;
		currentDiag = diagStart;
	}

	/// <summary>
	/// Set the temporary dialogue that will be used when this NPC has finished their current dialogue. Is prioritised over secondary dialogue.
	/// </summary>
	/// <param name="sp"></param>
	public void SetTemporaryDialogue(string sp, int sds = 0) {
		if (tempDiagPath is not null) {
			GD.Print("WARNING: Erasing previous temporary dialogue!!");
		}
		tempDiagPath = sp;
		tempDiagStart = sds;
	}

	/// <summary>
	/// Set the dialogue that will be used when this NPC has finished their current dialogue. Is not prioritised.
	/// </summary>
	/// <param name="sp"></param>
	public void SetSecondaryDialogue(string sp, int sds = 0) {
		if (secondaryDiagPath is not null) {
			GD.Print("WARNING: Erasing previous secondary dialogue!!");
		}
		secondaryDiagPath = sp;
		secondaryDiagStart = sds;
	}

	/// <summary>
	/// Load the dialogue to be used when the primary dialogue has finished. Prioritises loading temporary dialogue over secondary dialogue.
	/// </summary>
	public void LoadWaitingDialogue(int diagNum = -1) {
		if (diagNum == -1) diagNum = currentDiag;
		// GD.Print("1111111111111111111111111111111");
		// GD.Print(diagPath);
		if (tempDiagPath != null) {
			isTalking = true;
			LoadDialogue(tempDiagPath, tempDiagStart);
			tempDiagPath = null;
			tempDiagStart = 0;
		} else if (secondaryDiagPath != null) {
		// if (secondaryDiagPath != null && isTalking == false) {
		// if (secondaryDiagPath != null && diagNum == dialogue.Count) {
			LoadDialogue(secondaryDiagPath, secondaryDiagStart);
			// GD.Print(currentDiag);
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
			// LoadWaitingDialogue();
			var d = dialogue[currentDiag++];
			isTalking = true;
			return d;
		} else {
			RestartDialogue();
			LoadWaitingDialogue();
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
	/// Reset the dialogue to a specified dialogue scene from the first dialogue scene.
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
			ResetDialogue(diagPath);
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
