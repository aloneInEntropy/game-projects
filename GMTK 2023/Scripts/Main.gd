extends Control

onready var main_menu = $MainMenu
onready var title = $MainMenu/Title
onready var start_button = $MainMenu/StartButton
onready var quit_button = $MainMenu/QuitButton
onready var instructions = $Instructions
onready var next_button = $Instructions/NextButton
onready var instruction_script = $Instructions/Instruct
onready var controls = $Controls
onready var play_button = $Controls/PlayButton
onready var control_script = $Controls/Controls
# onready var stage = $Stage



func _ready():
	# stage.game_over = true
	# stage.visible = false
	# stage.gui.visible = false
	# stage.set_process(false)
	# stage.set_process_input(false)
	# stage.set_physics_process(false)
	main_menu.visible = true
	start_button.disabled = false
	quit_button.disabled = false
	instructions.visible = false
	instruction_script.visible = false
	play_button.disabled = true
	controls.visible = false
	control_script.visible = false
	next_button.disabled = true


func _on_StartButton_pressed():
	title.visible = false
	start_button.disabled = true
	quit_button.disabled = true
	main_menu.visible = false

	instructions.visible = true
	instruction_script.visible = true
	next_button.disabled = false
	GM.play_action_audio()
	# action_audio.play()

func _on_NextButton_pressed():
	instructions.visible = false
	instruction_script.visible = false
	next_button.disabled = true

	controls.visible = true
	control_script.visible = true
	play_button.disabled = false
	# action_audio.play()
	GM.play_action_audio()
	

func _on_PlayButton_pressed():
	GM.play_action_audio()
	yield(GM.action_audio, "finished")
	instructions.visible = false
	instruction_script.visible = false
	play_button.disabled = true
	print(get_tree().change_scene("res://Scenes/Levels.tscn"))

func _on_QuitButton_pressed():
	GM.play_action_audio()
	get_tree().quit()


