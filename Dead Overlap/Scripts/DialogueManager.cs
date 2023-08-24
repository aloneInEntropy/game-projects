using Godot;
using System;
using System.Collections.Generic;


/// <summary>
/// The global DialogueManager class. Handles dialogue events.
/// </summary>
public partial class DialogueManager : Node
{
	/// <summary>
	/// The DialogueBox object displayed when dialogue is active.
	/// </summary>
	public static DialogueBox activeDialogueBox = new();

	/// <summary>
	/// The DialogueObject currently displayed in the dialogue box.
	/// </summary>
	public static DialogueObject activeDialogue = new();

	/// <summary>
	/// How long to wait in frames before the next dialogue character update occurs.
	/// </summary>
	public static int framesBeforeUpdating = 20;

	/// <summary>
	/// How many characters are shown on each dialogue character update.
	/// </summary>
	public static float updateSpeechSpeed = 1;

	/// <summary>
	/// Is dialogue currently being read out to the dialogue box (i.e., typed out character by character or fully displayed?).
	/// </summary>
	public static bool isDialogueReading = false;
	
	/// <summary>
	/// How long in frames to stop dialogue from updating when at the end of a sentence (i.e., finds the characters '!', '?', or '.').
	/// </summary>
	int sentenceEndPause = 10;
	int sentenceEndPauseRate = 0;

	/// <summary>
	/// How long in frames to stop dialogue from updating when at a sentence break (i.e., finds the characters ',', ';', ':').
	/// </summary>
	int sentenceBreakPause = 5;
	int sentenceBreakPauseRate = 0;

	string[] sentenceEnds = { ".", "!", "?", ":" };
	string[] sentencePauses = { ",", ";", "-" };

	public override void _Ready()
    {
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* 
		This is a fix for a bug caused by a few disconnected factors.
		
		When loading a file using Parse(file_name), the method strips each line's end whitespace before continuing. The file end.txt has no characters to display and so returns an empty string.
		Due to some weird fuckery surrounding the RichTextLabel's visible character and ratio fields, this allows it to set visible characters to 0 (the total amount of characters) and visible ratio to 1 (all characters are being displayed). Somehow, this carries over in the dialogue box because it is no longer being deleted and recreated every time it closes, only reset manually.

		As for why this bug didn't appear consistently, I can only assume it had something to do with load times.
		 */
		if (IsInstanceValid(activeDialogueBox)) {
			if (activeDialogueBox.txt.VisibleCharacters == 0) {
				if (activeDialogueBox.txt.VisibleRatio == 1) {
					GD.Print($"error from {activeDialogueBox.dialogue.originFilePath}");
				}
				activeDialogueBox.txt.VisibleRatio = 0;
			}
			activeDialogueBox.finishedMarker.Visible = !isDialogueReading;
		}

		if (!GameManager.isGamePaused) {
			if (isDialogueReading) {
				// GD.Print(activeDialogueBox.txt.Text.Length);
				if (activeDialogueBox.txt.VisibleRatio != 1 && Array.Exists(sentenceEnds, element => element == activeDialogueBox.txt.Text[activeDialogueBox.txt.VisibleCharacters].ToString())) {
					// GD.Print("pause here");
					sentenceEndPauseRate = sentenceEndPause;
					UpdateVisibleText(); // display text one more time to flush character and avoid loop
				}
				if (activeDialogueBox.txt.VisibleRatio != 1 && Array.Exists(sentencePauses, element => element == activeDialogueBox.txt.Text[activeDialogueBox.txt.VisibleCharacters].ToString())) {
					// GD.Print("break here");
					sentenceBreakPauseRate = sentenceBreakPause;
					UpdateVisibleText(); // display text one more time to flush character and avoid loop
				}
				sentenceEndPauseRate = Mathf.Clamp(sentenceEndPauseRate - 1, 0, sentenceEndPause);
				sentenceBreakPauseRate = Mathf.Clamp(sentenceBreakPauseRate - 1, 0, sentenceBreakPause);
				if (sentenceEndPauseRate <= 0 && sentenceBreakPauseRate <= 0) {
					UpdateVisibleText(); // only update visible text when there are no pauses.
				}
			} 
			// else {
			// 	if (GameManager.frame - d == 20) {
			// 		isDialogueReading = true;
			// 	}
			// }
		}
	}

	/// <summary>
	/// Parse the text file at <c>txtPath</c> as dialogue. All files are assumed to be in the folder "res://Assets/Text/Dialogue/".
	/// </summary>
	/// <param name="txtPath">
	/// description
	/// </param>
	/// <returns>
	/// 	<p>
	///		A <c>List</c> of <c>DialogueObject</c>s for the dialogue file that was parsed.
	/// 	</p>
	/// </returns>
	public static List<DialogueObject> Parse(string txtPath) {
		List<DialogueObject> dobjs = new();
		FileAccess file = FileAccess.Open(Globals.resPathToDialogue + txtPath, FileAccess.ModeFlags.Read);
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
				} else if (fn.StartsWith("~") && fn[1..].ToString().IsValidInt()) {
					// GD.Print("choice dialogue start");
					adding_reg_dialogue = true;
					reg_dialogue_marker = fn[1..].ToString().ToInt();
					// GD.Print(string.Format("{0}, {1}, {2}", txt_path, reg_dialogue_marker, dobjs.Count));
				} else if (fn.StartsWith("##") && fn[2].ToString().IsValidInt()) {
					// GD.Print("dialogue option start");
				} else if (fn.StartsWith("@") && fn[1..].ToString().IsValidInt()) {
					// GD.Print("dialogue choice");
					adding_choice_dialogue = true;
					choice_dialogue_marker = fn[1..].ToString().ToInt();
				} else if (fn.StartsWith("##.")) {
					// GD.Print("dialogue option end");
				} else if (fn.StartsWith("#@") && fn[2..].ToString().IsValidInt()) {
					// GD.Print("post-dialogue choice option");
					adding_response_dialogue = true;
					response_dialogue_marker = fn[2..].ToString().ToInt();
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
				tdo.originFilePath = txtPath;
				if (!dobjs.Contains(tdo)) dobjs.Add(tdo);

				if (adding_reg_dialogue) {
					// If there are extra functions or signals to call during dialogue, parse them here.
					if (fnc_find != -1) {
						tdo.AddDialogue(line[..fnc_find]);
						string fnc = line[fnc_find + 2].ToString();
						switch (fnc) {
							case "|":
								tdo.AddDialogueFunction("EndDialogueB", 0); // end dialogue immediately
								break;
                            case "j":
								// end dialogue scene after displaying current dialogue
								tdo.AddDialogueFunction("ParseB " + line[(fnc_find + 4)..] + " s=false", 0);
								break;
                            case "e":
								// end dialogue scene after displaying current dialogue
								tdo.AddDialogueFunction("ParseB end.txt l=true p=0 s=false", 0); 
								break;
                            case "l":
								tdo.AddDialogueFunction("ParseB " + line[(fnc_find + 4)..], 0); // load dialogue file
								break;
							case "c":
								tdo.AddDialogueFunction("Modify " + line[(fnc_find + 4)..], 0); // call function
								break;
                            case "f":
								tdo.AddDialogueFunction(line[(fnc_find + 4)..], 0); // call function
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
							case "j":
								// jump to dialogue scene immediately without saving file
								tdo.AddChoiceFunction("ParseB " + line[(fnc_find + 4)..] + " s=false", choice_dialogue_marker); 
								break;
							case "e":
								// end dialogue scene after displaying current dialogue
								tdo.AddChoiceFunction("ParseB end.txt l=true p=0 s=false", choice_dialogue_marker); 
								break;
                            case "l":
								tdo.AddChoiceFunction("ParseB " + line[(fnc_find + 4)..], choice_dialogue_marker);
								break;
							case "c":
								tdo.AddChoiceFunction("Modify " + line[(fnc_find + 4)..], choice_dialogue_marker); // call function
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
					// If there are extra functions or signals to call during dialogue, parse them here.
					if (fnc_find != -1) {
						tdo.AddResponse(line[..fnc_find], response_dialogue_marker);
						string fnc = line[fnc_find + 2].ToString();
						switch (fnc) {
							case "|":
								tdo.AddResponseFunction("EndDialogueB", response_dialogue_marker);
								break;
							case "j":
								// jump to dialogue scene immediately without saving file
								tdo.AddResponseFunction("ParseB " + line[(fnc_find + 4)..] + " s=false", response_dialogue_marker); 
								break;
							case "e":
								// end dialogue scene after displaying current dialogue
								tdo.AddResponseFunction("ParseB end.txt l=true p=0 s=false", response_dialogue_marker); 
								break;
                            case "l":
								tdo.AddResponseFunction("ParseB " + line[(fnc_find + 4)..], response_dialogue_marker);
								break;
							case "c":
								tdo.AddResponseFunction("Modify " + line[(fnc_find + 4)..], response_dialogue_marker); // call function
								break;
                            case "f":
								tdo.AddResponseFunction(line[(fnc_find + 4)..], response_dialogue_marker);
								break;
							case "s":
								// tdo.AddChoiceSignal(line[(fnc_find + 4)..], response_dialogue_marker);
								GD.Print("Response signal" + line[(fnc_find + 4)..]);
								break;
							default:
								break;
						}
						// GD.Print(fnc);
					} else {
						tdo.AddResponse(line, response_dialogue_marker);
					}
					// tdo.AddResponse(line, response_dialogue_marker);
				}
			}
		}
		file.Close();
		return dobjs;
	}
	
	/// <summary>
	/// Parse the text file at <c>txtPath</c> as dialogue.
	/// Non-static version of <c>Parse</c>.
	/// </summary>
	/// <param name="txtPath">
	/// description
	/// </param>
	/// <returns>
	/// 	<p>
	///		A <c>List</c> of <c>DialogueObject</c>s for the dialogue file that was parsed.
	/// 	</p>
	/// </returns>
	public List<DialogueObject> ParseB(string txt_path) {
		return Parse(txt_path);
	}

	/// <summary>
	/// Update the amount of visible text displayed in the dialogue box. If <c>finish</c> is true, display all the text immediately.
	/// </summary>
	/// <param name="finish"></param>
	public static void UpdateVisibleText(bool finish = false) {
		// GD.Print(activeDialogueBox.txt.Text.Length);
		if (finish) {
			activeDialogueBox.txt.VisibleCharacters = -1;
			activeDialogueBox.txt.VisibleRatio = 1;
		} else {
			if (GameManager.frame % framesBeforeUpdating == 0) {
				activeDialogueBox.txt.VisibleCharacters = (int)Mathf.Clamp(activeDialogueBox.txt.VisibleCharacters + updateSpeechSpeed, 0, activeDialogueBox.txt.Text.Length);
			}
			if (GameManager.frame % AudioManager.updateVoiceSpeed == 0) {
				AudioManager.PlayVoice(Globals.talkingNPC.voice);
			}
		}
		isDialogueReading = activeDialogueBox.txt.VisibleRatio != 1 && activeDialogueBox.txt.VisibleCharacters != -1;
	}

	/// <summary>
	/// Set the dialogue to display and update in the dialogue box <c>diagBox</c>. 
	/// </summary>
	/// <param name="diagBox"></param>
	/// <param name="frames_before_updating"></param>
	/// <param name="update_speech_speed"></param>
	/// <param name="update_voice_speed"></param>
	public static void SetDialogueToUpdate(DialogueBox diagBox, int frames_before_updating = 20, float update_speech_speed = 1, float update_voice_speed = 4) {
		framesBeforeUpdating = frames_before_updating;
		updateSpeechSpeed = update_speech_speed;
		AudioManager.SetUpdateVoiceSpeed(update_voice_speed);
		activeDialogueBox = diagBox;
		isDialogueReading = true;
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

	/// <summary>
	/// Close and end the dialogue session.
	/// </summary>
	public static void EndDialogue() {
		isDialogueReading = false;
		activeDialogue = null;
        Globals.gui.CloseDialogue();
	}
	
	/// <summary>
	/// Close and end the dialogue session.
	/// Non-static version of <c>EndDialogue</c>.
	/// </summary>
	public void EndDialogueB() {
		EndDialogue();
		// isDialogueActive = false;
		// activeDialogue = null;
	}

	public void Modify(string nameTitle) {
		activeDialogueBox.Modify(nameTitle);
	}
}
