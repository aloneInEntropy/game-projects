extends Control

onready var main_menu = $MainMenu
onready var start_button = $MainMenu/StartButton
onready var quit_button = $MainMenu/QuitButton
onready var play_button = $Instructions/PlayButton
onready var title = $MainMenu/Title
onready var instructions = $Instructions
onready var instruction_script = $Instructions/Instruct
# onready var stage = $Stage



func _ready():
	# stage.game_over = true
	# stage.visible = false
	# stage.gui.visible = false
	# stage.set_process(false)
	# stage.set_process_input(false)
	# stage.set_physics_process(false)
	instructions.visible = false
	instruction_script.visible = false
	play_button.disabled = true

func _on_StartButton_pressed():
	title.visible = false
	start_button.disabled = true
	quit_button.disabled = true
	main_menu.visible = false

	instructions.visible = true
	instruction_script.visible = true
	play_button.disabled = false

func _on_QuitButton_pressed():
	get_tree().quit()

func _on_PlayButton_pressed():
	instructions.visible = false
	instruction_script.visible = false
	play_button.disabled = true
	get_tree().change_scene("res://Scenes/Stage.tscn")

