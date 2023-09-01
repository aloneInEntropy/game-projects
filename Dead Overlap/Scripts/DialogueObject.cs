using Godot;
using System.Collections.Generic;


/// <summary>
/// The DialogueObject class. <br/>
/// Contains information about a dialogue scene, which has dialogue, choices, responses, and functions to run alongside aforementioned fields.
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
	/// Functions to run for a given choice. The index of each list corresponds to the choice number.
	/// </summary>
	public List<string> responseFunctions = new(); 

	/// <summary>
	/// The DialogueObjects obtained from the function <c>DialogueManager.Parse</c> from this object's functions.<br/>
	/// parseResult[0] = <c>[DialogueObject]</c> : the list of dialogue objects to load in<br/>
	///	parseResult[1] = <c>bool</c>: choose whether or not to immediately load parseResult[0]<br/>
	///	parseResult[2] = <c>int</c>: the dialogue number to start the above list from<br/>
	///	parseResult[3] = <c>bool</c>: choose whether or not to save parseResult[0] to the NPC
	/// </summary>
	public List<object> parseResult;
	/// <summary>
	/// The result of the functions ran by this object.
	/// </summary>
	public List<object> functionResult;

	/// <summary>
	/// The file path to this dialogue object.
	/// </summary>
	public string originFilePath = "INVALID"; 
	
	public Dictionary<string, string> dialogueChoices = new(); // Dictionary using NPC dialogue as keys and NPC choices as values
	public Dictionary<string, string> choiceResponses = new(); // Dictionary using NPC choices as keys and NPC responses as values
	

	public void AddDialogue(string ch) {
		dialogue.Add(ch);
		dialogueFunctions.Add("");
	}
	
	public string GetDialogue(int v) {
		return dialogue[v];
	}

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

	// Add a response `ch` to the choice `addto`.
	public void AddResponse(string ch, int addto) {
		responses.Add(ch);
		responseFunctions.Add("");
		choiceResponses[choices[addto]] = ch;
	}
	
	public string GetResponse(int v) {
		return responses[v];
	}
	
	/// <summary>
	/// Add a bar separated (`|`) list of functions and space separated list of parameters <c>ch</c> to be called when the dialogue <c>d</c> is loaded.<br/>
	/// For example, <c>ch</c> == "shakeScreen 4|darkenScreen 0.5 3 6|updateSpeechSpeed 20" and <c>d</c> == 3
	/// </summary>
	public void AddDialogueFunction(string ch, int d) {
		// GD.Print(ch);
		dialogueFunctions[d] = ch;
	}

	/// <summary>
	/// Add a bar-separated (`|`) list of functions and space separated list of parameters `ch` to be called when the <c>choice</c> is selected.
	/// For example, `ch` == "shakeScreen 4|darkenScreen 0.5 3 6|updateSpeechSpeed 20" and `choice` == 2
	/// </summary>
	public void AddChoiceFunction(string ch, int choice) {
		// GD.Print(ch);
		choiceFunctions[choice] = ch;
	}
	
	/// <summary>
	/// Add a bar-separated (`|`) list of functions and space separated list of parameters `ch` to be called when the response <c>resp</c> is selected.
	/// For example, `ch` == "shakeScreen 4|darkenScreen 0.5 3 6|updateSpeechSpeed 20" and `choice` == 2
	/// </summary>
	public void AddResponseFunction(string ch, int resp) {
		// GD.Print(ch);
		responseFunctions[resp] = ch;
	}

	// Add a bar-separated (`|`) list of functions and space-separated list of parameters `ch` to be called when the `choice` is selected.
	public void AddChoiceSignal(string ch, int choice) {
		choiceSignals[choice] = ch;
	}
	
	/// Call all functions specified for the `choice`.
	public void CallChoiceFunctions(int choice) {
		if (choiceFunctions[choice] != "") {
			var (parseRes, funcRes) = CallFunctions(choiceFunctions[choice]);
			parseResult = parseRes;
			functionResult = funcRes;
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
				var (parseRes, funcRes) = CallFunctions(d);
				parseResult = parseRes;
				functionResult = funcRes;
			} else {
				// GD.Print("No available functions for this line.");
			}
		}
	}
	
	/// <summary>
	/// Call all response functions specified for this DialogueObject.
	/// </summary>
	public void CallResponseFunctions(int resp) {
		// GD.Print(d);
		if (responseFunctions[resp] != "") {
			var (parseRes, funcRes) = CallFunctions(responseFunctions[resp]);
			parseResult = parseRes;
			functionResult = funcRes;
		} else {
			// GD.Print("No available functions for this line.");
		}
	}

	/// <summary>
	/// Calls all the functions given by the bar-separated (|) string <c>funcString</c>.
	/// </summary>
	/// <param name="funcString"></param>
	/// <returns>The tuple containing the results of all functions ran, and the result of any file parsing in the first slot of the tuple. </returns>
	public (List<object> parseRes, List<object> funcRes) CallFunctions(string funcString) {
		List<object> funcRes = new(), parseRes = new();
		var fns = funcString.Split("|");
		parseResult = null;
		functionResult = null;
		foreach (string fn in fns) {
			string tfn = fn.StripEdges();
			Godot.Collections.Array tps = new();
			string[] paras = tfn.Split(" ");
			if (paras[0] == "ParseB") {
				/* 
					arg 1 = string -> the file path of the dialogue scene
					arg 2 = bool -> choose whether or not to immediately load arg 0 (optional)
					arg 3 = int -> the dialogue number to start arg 0 from (optional)
					arg 4 = bool -> choose whether or not to save arg 1 to the NPC (if present, all arguments must be present)
				*/
				string dfile = paras[1]; // Choose which file to load in. (Required; here only for readability.)
				bool dload = true; // Immediately display loaded dialogue if specified, otherwise only load.
				int dpos = 0; // If a dialogue start position is not specified, default to 0.
				bool dsave = true; // Save a dialogue path to the talking NPC when loading.

				foreach (var p in paras[1..]) {
					var pr = p.Split("=");
					switch (pr[0]) {
						case "f":
							dfile = pr[1];
							break;
						case "l":
							dload = bool.Parse(pr[1]);
							break;
						case "p":
							dpos = pr[1].ToInt();
							break;
						case "s":
							dsave = bool.Parse(pr[1]);
							break;
					}
				}

				parseRes = new List<object> {
					DialogueManager.ParsePath(dfile),
					dload,
					dpos,
					dsave
				};
			} else if (paras[0] == "CompleteMission" || paras[0] == "ActivateMission") {
				DialogueManager dm = new();
				funcRes.Add(dm.Call(paras[0], paras[1..].Join(" ")));
			} else {
				DialogueManager dm = new();
				tps.AddRange(paras[1..]);
				funcRes.Add(dm.Callv(paras[0], tps));
			}
			// GD.Print(paras);
		}
		return (parseRes, funcRes);
	}
}
