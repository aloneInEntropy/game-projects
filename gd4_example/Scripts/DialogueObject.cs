using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The DialogueObject class. Contains information about a dialogue scene, which has dialogue, choices, responses, and functions to run alongside aforementioned fields.
/// </summary>
public partial class DialogueObject
{
	/// <summary> 
	/// NPC dialogue, which may be spoken to the player
	/// </summary>
	public List<string> dialogue = new(); 

	/// <summary> 
	/// Functions to run when this dialogue plays. The index of each list corresponds to the dialogue number.
	/// </summary>
	public List<string> dialogueFunctions = new(); 

	/// <summary> 
	/// NPC choices, offered to the player 
	/// </summary>
	public List<string> choices = new(); 

	/// <summary> 
	/// Functions to run for a given choice. The index of each list corresponds to the choice number.
	/// </summary>
	public List<string> choiceFunctions = new(); 

	/// <summary> 
	/// Signals to call for a given choice. The index of each list corresponds to the choice number.
	/// </summary>
	public List<string> choiceSignals = new(); 

	/// <summary> 
	/// NPC responses to the choices given. If there are no choices, there are no responses.
	/// </summary>
	public List<string> responses = new(); 

	/// <summary>
	/// The DialogueObjects obtained from the function <c>DialogueManager.Parse</c> from this object's functions.<br/>
	/// parseResult[0] = <c>[DialogueObject]</c> : the list of dialogue objects to load in<br/>
	///	parseResult[1] = <c>bool</c>: choose whether or not to immediately load parseResult[0]<br/>
	///	parseResult[2] = <c>int</c>: the dialogue number to start the above list from<br/>
	/// </summary>
	public List<object> parseResult;
	/// <summary>
	/// The result of the functions ran by this object.
	/// </summary>
	public object functionResult;

	/// <summary>
	/// The file path to this dialogue object.
	/// </summary>
	public string originFilePath = "INVALID"; 
	
	public readonly Dictionary<string, string> dialogueChoices = new(); // Dictionary using NPC dialogue as keys and NPC choices as values
	public readonly Dictionary<string, string> choiceResponses = new(); // Dictionary using NPC choices as keys and NPC responses as values
	

	public void AddDialogue(string ch) {
		dialogue.Add(ch);
		dialogueFunctions.Add("");
	}
	
	public string GetDialogue(int v) {
		return dialogue[v];
	}

	// public void SetChoiceAmount(int v) {
	// 	vchoices = new string[v];
	// }

	// public void AddChoiceV(string ch, int pos) {
	// 	vchoices[pos] = ch;
	// }

	public void AddChoice(string ch, int addto) {
		// GD.Print(addto);
		// dialogueChoices[dialogue[addto]] = ch;
		choices.Add(ch);
		choiceFunctions.Add("");
		choiceSignals.Add("");
	}
	
	public string GetChoice(int v) {
		return choices[v];
	}
	
	
	/// <summary>
	/// Add a bar separated (`|`) list of functions and space separated list of parameters <c>ch</c> to be called when the dialogue <c>d</c> is loaded.<br/>
	/// For example, <c>ch</c> == "shakeScreen 4|darkenScreen 0.5 3 6|updateSpeechSpeed 20" and <c>d</c> == 3
	/// </summary>
	/// <param name="ch"></param>
	/// <param name="d"></param>
	public void AddDialogueFunction(string ch, int d) {
		dialogueFunctions[d] = ch;
	}

	/// Add a bar-separated (`|`) list of functions and space-separated list of parameters `ch` to be called when the `choice` is selected.
	/// For example, `ch` == "shakeScreen 4|darkenScreen 0.5 3 6|updateSpeechSpeed 20" and `choice` == 2
	public void AddChoiceFunction(string ch, int choice) {
		choiceFunctions[choice] = ch;
	}
	
	/// Call all functions specified for the `choice`.
	public void CallChoiceFunctions(int choice) {
		if (choiceFunctions[choice] != "") {
			var fns = choiceFunctions[choice].Split("|");
			foreach (string fn in fns) {
				string tfn = fn.StripEdges();
				Godot.Collections.Array tps = new();
				string[] paras = tfn.Split(" ");
				if (paras[0] == "ParseB") {
					/* 
						arg 1 = string -> the file path of the dialogue scene
						arg 2 = bool -> choose whether or not to immediately load arg 0 (optional)
						arg 3 = int -> the dialogue number to start arg 0 from (optional)
					*/
					var dload = true; // Immediately display loaded dialogue if specified, otherwise only load.
					var dpos = 0; // If a dialogue start position is not specified, default to 0.
					if (paras.Length == 3) {
						if (paras[2].IsValidInt()) {
							dpos = paras[2].ToInt();
						} else {
							dload = bool.Parse(paras[2]);
						}
					} else if (paras.Length == 4) {
						dload = bool.Parse(paras[2]);
						dpos = paras[3].ToInt();
					}

					parseResult = new List<object>{
						DialogueManager.Parse(paras[1]),
						dload,
						dpos
					};
				} else {
					DialogueManager dm = new();
					tps.AddRange(paras[1..]);
					functionResult = dm.Callv(paras[0], tps);
				}
				// GD.Print(paras);
			}
		} else {
			// GD.Print("No available functions for this choice.");
		}
	}
	

	/// <summary>
	/// Call all dialogue functions specified for this DialogueObject.
	/// </summary>
	public void CallDialogueFunctions() {
		// GD.Print(d);
		foreach (var d in dialogueFunctions) {
			if (d != "") {
				var fns = d.Split("|");
				foreach (string fn in fns) {
					string tfn = fn.StripEdges();
					Godot.Collections.Array tps = new();
					string[] paras = tfn.Split(" ");
					if (paras[0] == "ParseB") {
						/* 
							arg 1 = string -> the file path of the dialogue scene
							arg 2 = bool -> choose whether or not to immediately load arg 0 (optional)
							arg 3 = int -> the dialogue number to start arg 0 from (optional)
						*/
						var dload = true; // Immediately display loaded dialogue if specified, otherwise only load.
						var dpos = 0; // If a dialogue start position is not specified, default to 0.
						if (paras.Length == 3) {
							if (paras[2].IsValidInt()) {
								dpos = paras[2].ToInt();
							} else {
								dload = bool.Parse(paras[2]);
							}
						} else if (paras.Length == 4) {
							dload = bool.Parse(paras[2]);
							dpos = paras[3].ToInt();
						}

						parseResult = new List<object>{
							DialogueManager.Parse(paras[1]),
							dload,
							dpos
						};
					} else {
						DialogueManager dm = new();
						tps.AddRange(paras[1..]);
						functionResult = dm.Callv(paras[0], tps);
					}
					// GD.Print(paras);
				}
			} else {
				// GD.Print("No available functions for this line.");
			}
		}
	}

	// Add a bar-separated (`|`) list of functions and space-separated list of parameters `ch` to be called when the `choice` is selected.
	public void AddChoiceSignal(string ch, int choice) {
		choiceSignals[choice] = ch;
	}
	
	// Add a response `ch` to the choice `addto`.
	public void AddResponse(string ch, int addto) {
		responses.Add(ch);
		// GD.Print(choices.Count);
		// GD.Print(addto);
		choiceResponses[choices[addto]] = ch;
	}
	
	public string GetResponse(int v) {
		return responses[v];
	}
}
