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

- [] interactable class?
    - it would be a class of objects the player is able to interact with by being near, such as a chest or NPC. the NPC class would inherit it.

- [ ] player
    - [x] player sprites + animations
    - [x] player attacks
    - [ ] player weapons + damage (part of inventory)

- [ ] missions
    - [x] MissionType class
    - [ ] let NPCs track which dialogue "scenes" they've already gone through
    - [ ] dynamic mission completion detection

- [ ] inventory
    - [ ] give NPCs items from the inventory
    - [ ] give NPCs an inventory stock

- [ ] bartering
    - [ ] buy items from NPCs
    - [ ] sell items to NPCs

- [ ] enemies
    - [ ] enemy loot

- [ ] general loot
    - [ ] chests
    - [ ] treasure map
