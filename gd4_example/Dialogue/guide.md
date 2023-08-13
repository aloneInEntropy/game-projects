The text parser is still under construction, but the basics work like below. All text will be fed into Godot's bbcode system, to handle more complex/annoying cases dealing with things like bold and italic fonts. 

Please note that **ALL NUMBERS ARE *ZERO-INDEXED***. Assume that not following this **exact structure** will crash the dialogue parser.

## Cheat Sheet
`// ...`: Comment the whole line\
`~[dialogue number]`: Regular dialogue\
`~~[dialogue number]`: Choice dialogue\
`~##[choice amount]`: Choice section and amount of choices\
`~@[choice number]`: Choice number\
`~##.`: End of choice section\
`~#@[choice number]`: Response to choice number\
`~~.`: End of choice dialogue\
`~.`: End of dialogue

For the following commands, anything involving parsing a file (i.e., `||l`, `||j`) will require parameter signifiers for each parameter. File paths will require extensions as well (e.g., `||l f=first.txt`).

`... ||f [function name] [parameter1] [parameter2]`: call function during dialogue\
`... ||s [signal name] [parameter1] [parameter2]`: call signal during dialogue\
`... ||f/s [function/signal name] [parameter1] [parameter2]|[function/signal name] [parameter1]`: call multiple functions/signals during dialogue\
`... ||j f=[file path]] p=[dialogue number]`: shorthand to jump to dialogue line without saving path\
`... ||l f=[file path]] l=[load immediately?] p=[dialogue number] s=[save path?]`: 
>	`file path` (`str`) - path to file from `"res://Dialogue/"`\
	`load immediately?` (`bool`) - should the new dialogue be read immediately or after the current DialogueObject's dialogue is finished? (defaults to `true`)\
	`dialogue number` (`int`) - the position in the `file path` to load dialogue from (defaults to `0`)\
	`save path?`  (`bool`) -  should the new path be saved to the NPC's current dialogue or switch back afterwards? (defaults to `true`)\

`... ||e`: shorthand to end dialogue after current dialogue is finished
`... |||`: shorthand to end dialogue immediately



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
***[Note that all functions here are explicitly preceded by a tilde, but are not shown in explanations.]***


Choices are dictated using two hashtags (`##`) followed by the number of choices present. On the next line, each choice is written using the `@` symbol followed by the choice number. The choice section is then ended with another double hashtag followed by a period. 

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
After choices have been made, there may be dialogue following each choice. This is on the line immediately after the choices as a (`~`) followed by a hashtag (`#`) and an `@` and the choice the dialogue is in response to.

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
Finally, to end the choice section, simply add a tilde, hash, and period (`~#.`) to the line after the Response section (if there is one) and continue as normal.

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

