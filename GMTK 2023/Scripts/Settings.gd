extends Control

onready var show_line_button = $GuideLine/ShowLineButton
onready var show_mm_line_button = $GuideLine/ShowMinimapLineButton
onready var gui = get_parent() # the parent will always be the GUI
onready var stage = gui.get_parent() # the parent will always be the GUI

func _ready():
	show_mm_line_button.pressed = gui.bline.visible
	show_line_button.pressed = stage.best_line.visible

func _on_ReturnButton_pressed():
	queue_free()

func _on_ShowLineButton_toggled(button_pressed: bool):
	stage.best_line.visible = button_pressed

func _on_ShowMinimapLineButton_toggled(button_pressed: bool):
	gui.bline.visible = button_pressed
