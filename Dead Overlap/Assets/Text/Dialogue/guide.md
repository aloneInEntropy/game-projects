The text parser is still under construction, but the basics work like below. All text will be fed into Godot's bbcode system, to handle more complex/annoying cases dealing with things like bold and italic fonts. 

Please note that **ALL NUMBERS ARE *ZERO-INDEXED*** and ***ALL PATHS/NAMES ARE CASE-SENSITIVE***. Assume that not following this **exact structure** will crash the dialogue parser.
Also note that the dialogue box can currently fit 342 characters before overflowing. Make sure to compensate for longer words that may break onto the next line.

# Dialogue
## Cheat Sheet
### Syntax
`// ...`: Comment the whole line\
`~[dialogue number]`: Regular dialogue\
`~~[dialogue number]`: Choice dialogue\
`~##[choice amount]`: Choice section and amount of choices\
`~@[choice number]`: Choice number\
`~##.`: End of choice section\
`~#@[choice number]`: Response to choice number\
`~~.`: End of choice dialogue\
`~.`: End of dialogue

### Commands
For the following commands, anything involving parsing a file (i.e., `||l`, `||j`) will require parameter signifiers for each parameter. File paths will require extensions as well (e.g., `||l f=first.txt`).

`... ||f [function name] [parameter1] [parameter2]`: call function during dialogue\
`... ||s [signal name] [parameter1] [parameter2]`: call signal during dialogue\
`... ||f/s [function/signal name] [parameter1] [parameter2] | [function/signal name] [parameter1]`: call multiple functions/signals during dialogue

`... ||l f=[file path] l=[load immediately?] p=[dialogue number] s=[save path?]`\
`f` (`str`) - path to file from `"Assets/Text/Dialogue/"`\
`l` (`bool`) - should the new dialogue be read immediately next or after the current DialogueObject's dialogue is finished? When used at the end of a dialogue file, setting this to `false` waits for the dialogue box to close before loading it in. Otherwise, it is loaded in as the next dialogue object. (defaults to `true`)\
`p` (`int`) - the position in the `file path` to load dialogue from (defaults to `0`)\
`s`  (`bool`) -  should the new path be saved to the NPC's current dialogue or switch back afterwards? (defaults to `true`)

`... ||j f=[file path] l=[load immediately?] p=[dialogue number]` (***BROKEN! DO NOT USE.***)\
Command to jump to dialogue line without saving path. Shorthand for `||l f=[file path] l=[load immediately?] p=[dialogue number] s=false`

`... ||e`\
Command to end dialogue after current dialogue is finished. Shorthand for `||l f=end.txt l=false p=0 s=false`

`... |||`\
Command to end dialogue immediately. Shorthand for `||f EndDialogueB`

`||c [chara name]`: Command to switch characters.\
The name `chara name` will be the "true name" of an NPC. If "null", the box title will be an empty string. Short hand for `||f Modify chara_name`.\
Note that this will cause an error if the NPC `chara_name` has not yet been loaded in any previous scene. Once loaded, they persist between scenes.



## Regular Dialogue
Regular dialogue is all done on one line. Any line breaks are added directly. It is accessed using a single tilde (`~`) followed by the dialogue number.
```
~[dialogue number]
```
So, for example,
```
~0
Hello world! This is the first sentence.
~1
This is the second sentence.
~2
This is the third sentence.
```
... and so on.

You can end the file with `~.` at the very end of the file.


## Dialogue Choices
### Prompt
The preceding dialogue is introduced using a double tilde (`~~`).
```
~~[dialogue number]
```
Note that the dialogue number follows the previous dialogue number, even if it wasn't a dialogue choice.

So, for example,
```
~0
Hello world! This is the first sentence. 
~~1
What would you like for dinner?
```

### Choices


Choices are dictated using two hashtags (`~##`) followed by the number of choices present. On the next line, each choice is written using the `~@` symbol followed by the choice number. The choice section is then ended with another double hashtag followed by a period. 

To run a function during the dialogue, append the choice text with `||f` followed by a space and the name of the function. Separate parameters with spaces and further functions with another bar (`|`).
To call a signal during the dialogue, append the choice text with `||s` followed by a space and the name of the signal. Separate parameters with spaces and further signals with another bar (`|`).
To end the dialogue immediately, add three bars `|||` after the dialogue. This is shorthand for `||f endDialogueB`.

So, for example,
```
// ...
~##5
	~@0
		Pasta.
	~@1
		Pizza. ||s updateHunger 30
	~@2
		Pie. ||s updateHunger 15|updateHealth -15 9
	~@3
		Quiche. ||f shakeScreen 4|darkenScreen 0.5|updateSpeechSpeed 20
	~@4
		I'm not hungry, thank you. |||
~##.
```

### Responses
After choices have been made, there may be non-branching dialogue (a single immediate dialogue line too insignificant to warrant another dialogue file) following each choice. This is on the line immediately after the choices as a (`~#@`) and the choice the dialogue is in response to.

So, for example.
```
// ...
~##5
	~@0
		Pasta.
		~#@0
			Pasta, huh! My favourite! Quick and easy to make, and such great taste!
	~@1
		Pizza. ||s updateHunger 30
		~#@1
			...Pizza, huh? Who's gonna pay for that, d'ya think? I... what, you can't possibly be *that* hungry! God, you're bleedin' me dry.
	~@2
		Pie. ||s updateHunger 15|updateHealth -15 9
		~#@2
			Pie? I think, we have some in the fridge... don't think it's gonna fill ya up all that much, though.
	~@3
		Quiche. ||f shakeScreen 4|darkenScreen 0.5|updateSpeechSpeed 20
		~#@3
			Quiche? QUICHE? YOU ACTUALLY LIKE THAT STUFF?! EWW! It's in the fridge.
	~@4
		I'm not hungry, thank you. |||
~##.
```

### End choices
Finally, to end the choice section, simply add a tilde, hash, and period (`~##.`) to the line after the Response section (if there is one) and continue as normal.

All in all, the dialogue for choices will look like this:
```
~0
	Hello world! This is the first sentence. 
~~1
	What would you like for dinner?
		~##5
			~@0
				Pasta.
				~#@0
					Pasta, huh! My favourite! Quick and easy to make, and such great taste!
			~@1
				Pizza. ||s updateHunger 30
				~#@1
					...Pizza, huh? Who's gonna pay for that, d'ya think? I... what, you can't possibly be *that* hungry! God, you're bleedin' me dry.
			~@2
				Pie. ||s updateHunger 15|updateHealth -15 9
				~#@2
					Pie? I think, we have some in the fridge... don't think it's gonna fill ya up all that much, though.
			~@3
				Quiche. ||f shakeScreen 4|darkenScreen 0.5|updateSpeechSpeed 20
				~#@3
					Quiche? QUICHE? YOU ACTUALLY LIKE THAT STUFF?! EWW! It's in the fridge.
			~@4
				I'm not hungry, thank you. |||
		~##.
	~~.
~2
	Well, that was nice, I suppose. I'm headin' to bed. See ya in tha mornin'.
~.
```

A dialogue file detailing a series of choices the user can choose from. \
\
`Dialogue line 1`\
Hello world! This is the first sentence. \
`Dialogue line 2, choices incoming`\
What would you like for dinner?\
5 choices for dialogue line 2\
`Choice 1`\
Pasta.\
`Response to Choice 1`\
Pasta, huh! My favourite! Quick and easy to make, and such great taste!\
`Choice 2`\
Pizza. call signal update Hunger param 30\
`Response to Choice 2`\
...Pizza, huh? Who's gonna pay for that, d'ya think? I... what, you can't possibly be *that* hungry! God, you're bleedin' me dry.\
`Choice 3`\
Pie. call signal update Hunger param 15 call signal update Health param -15 param 9\
`response to choice 3`\
Pie? I think, we have some in the fridge... don't think it's gonna fill ya up all that much, though.\
`choice 4`\
Quiche. run function shake Screen param 4 run function darken Screen param 0.5 run function update Speech Speed param 20\
`response to choice 4`\
Quiche? QUICHE? YOU ACTUALLY LIKE THAT STUFF?! EWW! It's in the fridge.\
`Choice 5`\
I'm not hungry, thank you. run function end dialogue\
`end choices`\
`end dialogue line 2 and choice section`\
`dialogue line 3`\
Well, that was nice, I suppose. I'm headin' to bed. See ya in tha mornin'.\
`end dialogue`

## Comments
You can comment out (ignore) any line beginning with double forward slashes `//`.

For example,
```
~0
	Hello world.
	//I don't want this line to show up.
	Hello world 2.
~.
```

This will output
```
Hello world.
Hello world 2.
```




# Action Triggers
The ActionTrigger class is a class that allows methods to be run depending on the Player's actions. The actions are run from **dialogue scripts** in the Assets/Text/Dialogue folder. These actions are methods that must exist in a non-static context within the DialogueManager class in the Scripts folder. You can see the above Dialogue section for more information on how to use the dialogue parser. 

There are also optional conditions that must be met for the actions that, if not met, can trigger other actions. Additionally, triggers can be set to run automatically, without direct interaction. Right now, there are two types:
	- Interactable
	- Room Trigger
One of these two options must checked when using the ActionTrigger class.

## Interactables
Interactable objects are any object the Player can interact with. 

### Description Path (string)
When triggered, a specified dialogue script in the `descriptionPath` field will be run. The file location defaults to `Assets/Text/Dialogue/Interactables` and can be navigated out of using `../`.

### Describer (string)
Optionally, a chosen NPC can speak when the `describer` field is provided with that NPC's name. If not, it defaults to the Narrator NPC. This field has very little use in most cases, as the `||c` command in the given dialogue script will override anything entered into the field. Regardless, it exists if modifiying the dialogue box through the dialogue script is undesired.

## Room Trigger
Room triggers are triggers that change the current scene when triggered. 

### Scene Name (string)
The scene name is the name of the room (scene) you would like to switch to. This corresponds to the name of the .tscn file in the Scenes folder.

### Entry Point (Vector2)
The entry point is the location the Player starts in when the room is loaded. If an entry point is not specified (`entryPoint == Vector2.Inf`), the Player's spawn point will default to the `defaultEntryPoint` of that scene (see the Location class).

### Facing Direction (Vector2)
The position the Player is facing when the room is loaded. If a direction is not specified (`facingDirection == Vector2.Zero`, see `TryUpdateFacingPos()` in the ActionTrigger class) the direction defaults to the Player's facing direction upon trigger.

### Keep X Position (bool)
A boolean that decides whether or not to keep the Player's position on the x-axis when changing rooms. This is useful when the trigger is set to change rooms vertically and the current and next rooms are aligned. 

### Keep Y Position (bool)
A boolean that decides whether or not to keep the Player's position on the y-axis when changing rooms. This is useful when the trigger is set to change rooms horizontally and the current and next rooms are aligned. 

## Requirements
As previously mentioned, there are optional conditions that must be met for the trigger to work. If these conditions exist and are not met, this can in turn trigger other actions. At the moment, only one condition can be checked to determine validity.

### Trigger Requirement Variable Name (string)
The name of the variable to be checked when determining validity. This is stored in `Data/variables.json` under the `checks` field.

### Trigger Requirement Variable Value (bool)
The value of the variable to be checked when determining validity. If `true`, the above variable must be true for the check to be valid, and vice versa.

### Trigger Denied Text Source (string)
The path to the dialogue script to run if the above check fails. If left blank, no dialogue file is run. This is analogous to the Interactable description path.

### Trigger Denied Text Speaker (string)
The name of the chosen NPC to speak when the trigger requirement check fails. This is again optional and secondary to any modifications made in the dialogue script using the `||c` command. This too is analogous to the Interactable description path.

## Auto Triggers
If the `autoTrigger` field is true, then the Interactable or Room Trigger will trigger immediately when the Player's interact box touches this ActionTrigger's CollisionPolygon2D.
