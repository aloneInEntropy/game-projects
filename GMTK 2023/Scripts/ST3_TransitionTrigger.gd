extends Area2D

onready var stage := get_parent()

	
func _on_TransitionTrigger_body_entered(body: Node):
	if body == stage.chara:
		stage.camera.current = false
		stage.get_node("SecondCamera").make_current()
		stage.camera_follow.remote_path = stage.get_node("SecondCamera").get_path()
		stage.get_node("TileMap").visible = false
		stage.get_node("TileMap2").visible = true
		stage.tilemap = stage.get_node("TileMap2")
		# stage.init_nav_grid()
	
