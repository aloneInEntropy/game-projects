- [x] dialogue parser
    - [x] regular dialogue
        - [x] run functions/signals in dialogue
    - [x] choice dialogue
        - [x] run functions/signals in dialogue
    - [x] response dialogue
        - [x] run functions/signals in dialogue
        - [x] show response and exit (part of above goal)
    - [x] dialogue branching
        - [x] load dialogue in specified files at specified indices
        - [x] branching without permanantly loading in files (temporary branching). possibly an optional update (bool) parameter to NPC.LoadDialogue (it'll be 5 at that point)
        - [x] stopping `||e` command from saving into dialogue path (part of above goal). Main problem is the command being saved, causing a loop where the dialogue box is closed immediately upon opening, preventing any other functions from being run.
    - [x] specify shorthand command parameters (e.g., `||l f=d1_re.txt l=false p=0 s=true`)
    - [x] dialogue "voices"
    - [x] pause dialogue reading at sentence breaks (full stops, commas, etc.).
        - [ ] Godot's bbcode system does not display the special characters used for inline functions (e.g., [b]Olivia[/b] will not display the tags `b` and `/b` and will instead bold the text, as expected). This causes a minor problem where the NPC's voice appears delayed at random moments, when it otherwise should type out normally. A possible solution is to remove bbcode text entirely and bold, italicise, and colour text through my own inline functions, but that would take time and be more complicated than necessary. The alternative is removing any special text effects, which would be much easier but also kinda lame :(


- [ ] inventory
    - [ ] give NPCs items from the inventory
    - [ ] give NPCs an inventory stock

- [ ] missions
    - [ ] let NPCs track which dialogue "scenes" they've already gone through

- [ ] bartering
    - [ ] buy items from NPCs
    - [ ] sell items to NPCs

