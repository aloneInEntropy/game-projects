using Godot;
using System;

public partial class RoomTriggerShack : RoomTrigger
{
    
    /// <summary>
    /// Change the scene according to this RoomTrigger's variables. <br/>
	/// If <c>keepXPosition</c> or <c>keepYPosition</c> are set to <c>true</c>, the <c>entryPoint</c> will be ignored.
    /// </summary>
    public override void Change() {
        if (PlayerVariables.hasLight) {
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
                GetParent().GetParent().GetNode("Interactables").GetNode<Interactable>("ShackDoor").OpenDescription();
            }
        }
    }
}
