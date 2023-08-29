using Godot;
using System;

public partial class RoomTriggerShack : RoomTrigger
{
    /// <summary>
    /// Change the scene according to this RoomTrigger's variables. <br/>
	/// If <c>keepXPosition</c> or <c>keepYPosition</c> are set to <c>true</c>, the <c>entryPoint</c> will be ignored.
    /// </summary>
    public override void Change() {
        GD.Print("dsfdgfdsd");
        if (PlayerVariables.hasLight) {
            GetParent().GetParent().GetNode("Interactables").GetNode<Interactable>("ShackDoor").canInteractWith = false;
            GetTree().ChangeSceneToFile("res://Scenes/" + sceneName + ".tscn");
                GameManager.sceneChangePosition = new Vector2(
                keepXPosition ? Globals.player.Position.X : entryPoint.X,
                keepYPosition ? Globals.player.Position.Y : entryPoint.Y
            );
            GameManager.sceneChangeFacing = facingDirection;
        } else {
            if (DialogueManager.isDialogueReading) {
                // if dialogue is currently being typed out
                DialogueManager.UpdateVisibleText(true);
            } else {
                // Globals.gui.OpenDialogue("unavailableunavailableunavailableunavailableunavailableunavailableunavailableunavailableunavailable", true);
                // Globals.gui.db.Modify("Narrator");
                // Globals.gui.db.WriteCustom("unavailableunavailableunavailableunavailableunavailableunavailableunavailableunavailableunavailable unavailableunavailable");
                // GetParent().GetParent().GetNode("Interactables").GetNode<Interactable>("ShackDoor").OnAreaEntered(Globals.player.interactBox);
                // Globals.GetNPC("Narrator").LoadDialogue("Interactables/" + GetParent().GetParent().GetNode("Interactables").GetNode<Interactable>("ShackDoor").descriptionPath);
                GetParent().GetParent().GetNode("Interactables").GetNode<Interactable>("ShackDoor").OpenDescription();
                // GetParent().GetParent().GetNode("Interactables").GetNode<Interactable>("ShackDoor").canInteractWith = true;
                // Globals.GetNPC("Narrator").LoadDialogue("Interactables/town_east_shack_side.txt");
            }
        }
    }
}
