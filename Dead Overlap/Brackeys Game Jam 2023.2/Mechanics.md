# Menus
## Notebook
The Player will be able to highlight the text in dialogue boxes and save them into a notebook ordered by the name of the NPC speaking and the year the Player is in. This menu should be accessible at all times, similar to a minimap.

## Dialogue
There should be back-and-forth dialogue between NPCs and the player. One way to do this could be adding a new parameter to `||l` allowing for the changing of speaking NPCs or characters and adding a Player NPC to speak.
## `||l f=d4.txt t=[target speaker name]`
If the `target speaker name` is "Player", it's the Player's (Marceline Ren's) turn to speak.

## ``||c [target speaker name] | [target speaker name] | [target speaker name] | ...` 
A command at the start of a file. The bars separate the names of the NPCs who speak.