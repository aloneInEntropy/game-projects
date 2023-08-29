# Menus
## Notebook
The Player will be able to highlight the text in dialogue boxes and save them into a notebook ordered by the name of the NPC speaking and the year the Player is in. This menu should be accessible at all times, similar to a minimap.

## Dialogue
There should be back-and-forth dialogue between NPCs and the player. One way to do this could be adding a new parameter to `||l` allowing for the changing of speaking NPCs or characters and adding a Player NPC to speak.
## `||l f=d4.txt t=[target speaker name]`
If the `target speaker name` is "Player", it's the Player's (Marceline Ren's) turn to speak.

## ``||c [target speaker name] | [target speaker name] | [target speaker name] | ...` 
A command at the start of a file. The bars separate the names of the NPCs who speak.
If the `target speaker name` is "Player", it's the Player's (Marceline Ren's) turn to speak. 
If the `target speaker name` is otherwise not the name of an NPC, continue with the text as if it was off-screen or a narrator.
If the `target speaker name` is "null", hide the name and picture label.


## Room Traversal
Keep track of whether or not the Player is inside a building and, if so, keep track of the position they were at before entering in the GameManager. Then load the scene of that particular building, maybe making use of an Area2D RoomTrigger node or something similar.

## RoomTrigger Conditionals
Add conditions to RoomTrigger nodes to check whether or not to accept traversal.

## ActionTrigger class
Consider implementing an ActionTrigger class that combines the effects of the Interactable class and RoomTrigger class into one. This would require replacing all Interactable and RoomTrigger nodes.