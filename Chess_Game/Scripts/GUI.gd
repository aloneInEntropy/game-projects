extends CanvasLayer

onready var start_button = $StartButton
onready var quit_button = $QuitButton
onready var board = $Board
onready var board_gui = $Board/GUIBanner
onready var board_meta = $Board/MetaBanner
onready var title_image = $TextureRect
onready var title = $Title

func _ready():
	# board.visible = false
	# get_tree().paused = true
	board.can_start = false
	pass

func _on_StartButton_pressed():
	print("did something")
	board.visible = true
	board.can_start = true
	# board.get_tree().paused = false
	title.visible = false
	start_button.disabled = true
	start_button.visible = false
	title_image.visible = false


func _on_QuitButton_pressed():
	get_tree().quit()
