using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The DialogueObject class. Contains information about a dialogue scene, which has dialogue, choices, responses, and functions to run alongside aforementioned fields.
/// </summary>
public partial class DialogueObject
{
	public List<string> dialogue = new(); // NPC dialogue, which may be spoken to the player
	public List<string> dialogueFunctions = new(); // Functions to run when this dialogue plays. The index of each list corresponds to the dialogue number.
	public List<string> choices = new(); // NPC choices, offered to the player 
	public List<string> choiceFunctions = new(); // Functions to run for a given choice. The index of each list corresponds to the choice number.
	public List<string> choiceSignals = new(); // Signals to call for a given choice. The index of each list corresponds to the choice number.
	public List<string> responses = new(); // NPC responses to the choices given. If there are no choices, there are no responses.
	public readonly Dictionary<string, string> dialogueChoices = new(); // Dictionary using NPC dialogue as keys and NPC choices as values
	public readonly Dictionary<string, string> choiceResponses = new(); // Dictionary using NPC choices as keys and NPC responses as values
	public List<object> parseResult;
	public object functionResult;
	// public readonly Dictionary<string, string> choiceFunctions = new(); // Dictionary using NPC choices as keys and calfunctions as values
	// public string[] vchoices;
	

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
					// If a dialogue start position is not specified, default to 0.
					var dpos = paras.Length < 3 ? 
						0 : 
						paras[2].ToInt()
					;
					parseResult = new List<object>{
						DialogueManager.Parse(paras[1]),
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
	
	/// Call all functions specified for the dialogue `d`.
	// public void CallDialogueFunctions(int d) {
	// 	// GD.Print(d);
	// 	if (dialogueFunctions[d] != "") {
	// 		var fns = dialogueFunctions[d].Split("|");
	// 		foreach (string fn in fns) {
	// 			string tfn = fn.StripEdges();
	// 			Godot.Collections.Array tps = new();
	// 			string[] paras = tfn.Split(" ");
	// 			if (paras[0] == "ParseB") {
	// 				// If a dialogue start position is not specified, default to 0.
	// 				var dpos = paras.Length < 3 ? 
	// 					0 : 
	// 					paras[2].ToInt()
	// 				;
	// 				parseResult = new List<object>{
	// 					DialogueManager.Parse(paras[1]),
	// 					dpos
	// 				};
	// 			} else {
	// 				DialogueManager dm = new();
	// 				tps.AddRange(paras[1..]);
	// 				functionResult = dm.Callv(paras[0], tps);
	// 			}
	// 			// GD.Print(paras);
	// 		}
	// 	} else {
	// 		GD.Print("No available functions for this line.");
	// 	}
	// }

	/// Call all functions specified for the dialogue `d`.
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
						// If a dialogue start position is not specified, default to 0.
						var dpos = paras.Length < 3 ? 
							0 : 
							paras[2].ToInt()
						;
						parseResult = new List<object>{
							DialogueManager.Parse(paras[1]),
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
