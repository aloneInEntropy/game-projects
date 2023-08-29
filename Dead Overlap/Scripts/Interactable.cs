/// <summary>
/// The Interactable class. Defines any object that can be interacted with by the Player using the keyboard or mouse.
/// </summary>

using System.Collections.Generic;
using Godot;
public partial class Interactable : Area2D
{
    [Export(PropertyHint.MultilineText)]
    public string description = "";

    [Export(PropertyHint.MultilineText)]
    public string descriptionPath = "";

    [Export]
    public Texture showcase;

    public bool displayShowcase = false;

    /// <summary>
    /// Can this Interactable object be interacted with?
    /// </summary>
    [Export]
    public bool canInteractWith = true;
    // public List<DialogueObject> dobjs = new();

    public override void _Ready() {
        // dobjs = DialogueManager.Parse(description);
    }

    /// <summary>
    /// Open the description for this Interactable object and describe it. The voice and portrait for the dialogue box when opened is the Narrator by default.
    /// </summary>
    /// <param name="describer"></param>
    public void OpenDescription(string describer = "Narrator") {
        if (canInteractWith) {
            Globals.talkingNPC = Globals.GetNPC("Narrator");
            Globals.gui.ProgressDialogue(Globals.GetNPC("Narrator"));
            Globals.gui.db.Modify(describer);
        }
    }

    public void OnAreaEntered(Area2D area) {
        if (area.GetParent().GetType() == typeof(Player)) {
            if (descriptionPath != "") Globals.GetNPC("Narrator").LoadDialogue("Interactables/" + descriptionPath);
            else GD.Print("Nothing in this interactable");
        }
    }
}
