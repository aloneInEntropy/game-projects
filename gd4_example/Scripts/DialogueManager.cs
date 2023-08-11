using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueManager : Node
{
	public static List<DialogueObject> dialogue_objs = new(); // the order of dialogue as it is presented
	public static DialogueBox activeDialogueBox = new();
	public static int framesBeforeUpdating = 20;
	public static float updateSpeechSpeed = 1;
	public static bool isDialogueActive = false; // is dialogue currently being read?
	public static DialogueObject activeDialogue = new(); // dialogue currently being read

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isDialogueActive) UpdateVisibleText();
		if (IsInstanceValid(activeDialogueBox)) activeDialogueBox.finishedMarker.Visible = !isDialogueActive;
	}

	// 
	// Parse the text file at `txt_path` as dialogue.
	// 
	public static List<DialogueObject> Parse(string txt_path) {
		List<DialogueObject> dobjs = new();
		FileAccess file = FileAccess.Open(txt_path, FileAccess.ModeFlags.Read);
		bool adding_reg_dialogue = false; // are you adding regular dialogue? (~x marker)
		int reg_dialogue_marker = 0; // which regular dialogue are you currently parsing?
		bool adding_choice_dialogue = false; // are you adding choice dialogue? (~~x marker)
		int choice_dialogue_marker = 0; // which choice dialogue are you currently parsing?
		bool adding_response_dialogue = false; // are you adding response dialogue? (~#@x marker)
		int response_dialogue_marker = 0; // which response dialogue are you currently parsing?
		while (file.GetPosition() < file.GetLength()) {
			string line = file.GetLine().StripEdges();
			if (line.StartsWith('~')) {
				adding_reg_dialogue = false;
				adding_choice_dialogue = false;
				adding_response_dialogue = false;

				string fn = line[1..];
				if (fn.ToString().IsValidInt()) {
					// GD.Print("regular dialogue section");
					adding_reg_dialogue = true;
					reg_dialogue_marker = fn.ToString().ToInt();
					// GD.Print(string.Format("{0}, {1}, {2}", txt_path, reg_dialogue_marker, dobjs.Count));
				} else if (fn.StartsWith("~") && fn[1].ToString().IsValidInt()) {
					// GD.Print("choice dialogue start");
					adding_reg_dialogue = true;
					reg_dialogue_marker = fn[1].ToString().ToInt();
					// GD.Print(string.Format("{0}, {1}, {2}", txt_path, reg_dialogue_marker, dobjs.Count));
				} else if (fn.StartsWith("##") && fn[2].ToString().IsValidInt()) {
					// GD.Print("dialogue option start");
				} else if (fn.StartsWith("@") && fn[1].ToString().IsValidInt()) {
					// GD.Print("dialogue choice");
					adding_choice_dialogue = true;
					choice_dialogue_marker = fn[1].ToString().ToInt();
				} else if (fn.StartsWith("##.")) {
					// GD.Print("dialogue option end");
				} else if (fn.StartsWith("#@") && fn[2].ToString().IsValidInt()) {
					// GD.Print("post-dialogue choice option");
					adding_response_dialogue = true;
					response_dialogue_marker = fn[2].ToString().ToInt();
				} else if (fn == "~.") {
					// GD.Print("choice dialogue end");
				} else if (fn == ".") {
					// GD.Print("dialogue end");
				}
			} else if (!line.StartsWith("//")) {
				var fnc_find = line.Find("||"); // does this line have a function to run alonside it?
				var tdo = reg_dialogue_marker == dobjs.Count ? 
					new DialogueObject() : 
					dobjs[reg_dialogue_marker]
				;
				if (!dobjs.Contains(tdo)) dobjs.Add(tdo);

				if (adding_reg_dialogue) {
					// If there are extra functions or signals to call during dialogue, parse them here.
					if (fnc_find != -1) {
						tdo.AddDialogue(line[..fnc_find]);
						string fnc = line[fnc_find + 2].ToString();
						switch (fnc) {
							case "|":
								tdo.AddDialogueFunction("EndDialogueB", choice_dialogue_marker);
								break;
                            case "l":
								tdo.AddDialogueFunction("ParseB res://Dialogue/" + line[(fnc_find + 4)..], choice_dialogue_marker);
								break;
                            case "f":
								tdo.AddDialogueFunction(line[(fnc_find + 4)..], choice_dialogue_marker);
								break;
							case "s":
								// tdo.AddChoiceSignal(line[(fnc_find + 4)..], choice_dialogue_marker);
								GD.Print("Dialogue signal" + line[(fnc_find + 4)..]);
								break;
							default:
								break;
						}
					} else {
						tdo.AddDialogue(line);
					}
				} else if (adding_choice_dialogue) {
					// If there are extra functions or signals to call during dialogue, parse them here.
					if (fnc_find != -1) {
						tdo.AddChoice(line[..fnc_find], reg_dialogue_marker);
						string fnc = line[fnc_find + 2].ToString();
						switch (fnc) {
							case "|":
								tdo.AddChoiceFunction("EndDialogueB", choice_dialogue_marker);
								break;
                            case "l":
								tdo.AddChoiceFunction("ParseB res://Dialogue/" + line[(fnc_find + 4)..], choice_dialogue_marker);
								break;
                            case "f":
								tdo.AddChoiceFunction(line[(fnc_find + 4)..], choice_dialogue_marker);
								break;
							case "s":
								tdo.AddChoiceSignal(line[(fnc_find + 4)..], choice_dialogue_marker);
								break;
							default:
								break;
						}
						// GD.Print(fnc);
					} else {
						tdo.AddChoice(line, reg_dialogue_marker);
					}
				} else if (adding_response_dialogue) {
					tdo.AddResponse(line, response_dialogue_marker);
				}
			}
		}
		file.Close();
		return dobjs;
	}
	
	public List<DialogueObject> ParseB(string txt_path) {
		return Parse(txt_path);
	}


	public static void UpdateVisibleText(bool finish = false) {
		if (finish) {
			activeDialogueBox.txt.VisibleRatio = 1;
		} else {
			if (GameManager.frame % framesBeforeUpdating == 0) {
				activeDialogueBox.txt.VisibleCharacters = (int)Mathf.Clamp(activeDialogueBox.txt.VisibleCharacters + updateSpeechSpeed, 0, activeDialogueBox.txt.Text.Length);
			}
		}
		isDialogueActive = activeDialogueBox.txt.VisibleRatio != 1;
	}

	public static void SetDialogueToUpdate(DialogueBox diagBox, int frames_before_updating = 20, float update_speech_speed = 1) {
		framesBeforeUpdating = frames_before_updating;
		updateSpeechSpeed = update_speech_speed;
		activeDialogueBox = diagBox;
		isDialogueActive = true;
	}

	public void SayHello() {
		GD.Print("HELLO WORLD!");
	}
	
	public void Test1(string out1) {
		GD.Print(string.Format("HELLO {0}!", out1));
	}

	public void Test2(string out1, string out2) {
		GD.Print(string.Format("HELLO {0} and {1}!", out1, out2));
	}

	// Close and end the dialogue session.
	public static void EndDialogue() {
		isDialogueActive = false;
		activeDialogue = null;
	}
	
	// Close and end the dialogue session (non-static).
	public void EndDialogueB() {
        GUI g = (GUI)activeDialogueBox.Owner;
        g.CloseDialogue();
		// isDialogueActive = false;
		// activeDialogue = null;
	}
}
