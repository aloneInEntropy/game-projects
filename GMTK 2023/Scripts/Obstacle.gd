extends KinematicBody2D

onready var stage := get_parent().get_parent() # the stage the character is in
onready var navobs = $NavigationObstacle2D

var dragging = false
var mouseDir : Vector2
var limit_dist := 100
var mouse_snap : Vector2
var mouse_pos : Vector2
var mouse_snap_diff : Vector2

func _ready():
	Navigation2DServer.agent_set_map(navobs.get_rid(), get_world_2d().navigation_map)
	# stage.anim.play("move_ob_1")


func _process(_delta):
	pass

func _physics_process(_delta: float) -> void:
	mouse_pos = get_viewport().get_mouse_position()
	if dragging:
		position = stage.roundToN(mouse_pos - mouse_snap_diff, stage.gap)
		# stage.place_tile_global(stage.tilemap, position, 1)
		# move_and_collide( mouseDir * 10 )


func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_MIDDLE and !event.pressed:
			dragging = false
		

func _on_Obstacle_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton:
		if event.button_index == BUTTON_MIDDLE:
			dragging = event.pressed
			if dragging:
				mouse_snap_diff = Vector2(abs(position.x - mouse_pos.x), abs(position.y - mouse_pos.y))
				# print(mouse_snap_diff)
				# mouse_snap = mouse_pos

				
