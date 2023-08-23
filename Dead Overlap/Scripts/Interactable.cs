/// <summary>
/// The Interactable class. Defines any object that can be interacted with by the Player using the keyboard or mouse.
/// </summary>

using Godot;
public partial class Interactable : Area2D
{
    [Export]
    public string description = "";

    [Export]
    public Texture showcase;

    public bool displayShowcase = false;
    private bool hasShownDesc = false;

    public void OpenDescription() {
        GD.Print("opening");
        if (!hasShownDesc) {
            Globals.gui.OpenDialogue(description, false);
            hasShownDesc = true;
        } else {
            Globals.gui.CloseDialogue();
            hasShownDesc = false;
        }
    }
}
