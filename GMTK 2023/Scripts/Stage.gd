extends Node2D

onready var tilemap := $TileMap
onready var chara := $Chara
onready var best_line := $Line2D
onready var dtimer := $DeathTimer
onready var gui := $GUI
onready var label := $GUI/Label
onready var place_label := $GUI/PlaceLabel
onready var cut_label := $GUI/BreakLabel
onready var target_label := $GUI/TargetLabel
onready var time_label := $GUI/TimeLabel
onready var death_control := $GUI/DeathControl
onready var win_control := $GUI/WinControl
onready var restart_button := $GUI/RestartButton
onready var quit_button := $GUI/QuitButton
onready var anim = $AnimationPlayer
onready var anim2 = $AnimationPlayer2
onready var anim3 = $AnimationPlayer3
onready var anim4 = $AnimationPlayer4
onready var anim5 = $AnimationPlayer5
onready var camera = $Camera2D

var TILE_NAV = 0
var TILE_OTHER = 1
var TILE_SOLID = 2
var TILE_TARGET = 3
var TILE_USER = 4

var astar := AStar2D.new()
var rng := RandomNumberGenerator.new()
var game_over := false
var id_count := 0
var gap := 32
var move_timer_start := 1
var move_timer = move_timer_start
var tile_anim_timer_start := 30
var tile_anim_timer = tile_anim_timer_start
var mouse_pos : Vector2 # mosue position
var mouse_snapshot_pos : Vector2 # mouse position the moment it is pressed
var end_goal : Vector2
var end_goal_id : int
var move_object_held := -1 # id of moveable object held (-1 if none held)
var is_dragging := false
var map
var bpath := PoolVector2Array()
var frame := 0
var time := 0.0
var counter := 1
var smooth_var := false

var alteration_replacement_length := 300 # how long until a player alteration is reverted? (in frames)
var places := 5 # how many tiles can the player place?
var cuts := 5 	# how many tiles can the player remove?
var pre_player_data = {}

onready var target_positions := []
onready var retarget_positions := []

# signal chara_can_move(move_path)
signal chara_can_move(mpath)

func _ready():
	rng.randomize()
	mouse_pos = get_global_mouse_position()
	var rnd_x := rng.randi_range(0, 31)
	var rnd_y := rng.randi_range(0, 62)
	init_nav_grid(Vector2(rnd_x, rnd_y))
	end_goal = target_positions[0]

	restart_button.disabled = true
	quit_button.disabled = true
	restart_button.visible = false
	quit_button.visible = false
	death_control.visible = false
	win_control.visible = false

	place_label.text = "Places left: %s" % places
	cut_label.text = "Cuts left: %s" % cuts
	target_label.text = "Targets left: %s" % target_positions.size()

	dtimer.start(15)
	
	anim.play("something")
	anim2.play("something2")
	anim3.play("something3")
	anim4.play("something4")
	anim5.play("something5")
	
func _process(delta):
	if !game_over:
		time += delta
		time_label.text = "%.2f" % dtimer.time_left
		mouse_pos = get_global_mouse_position()
		# camera.position = chara.position

		# temporary death barrier, lasts 5 seconds
		if frame == (60*5):
			for c in tilemap.get_used_cells():
				if c.y >= 60 and tilemap.get_cell(c.x, c.y) == 1:
					place_tile_local(tilemap, c, 0)
		
		# every 3 seconds
		if frame % (60*3) == 0:
			cut_label.text = "Cuts left: %s" % cuts
			cuts += 1
			place_label.text = "Places left: %s" % places
			places += 1

		# every 60 seconds
		if frame % (60*60) == 0:
			if retarget_positions:
				place_tile_global(tilemap, retarget_positions[0], TILE_TARGET)


		# var bpath := astar.get_point_path(astar.get_closest_point(chara.position), end_goal_id) # astar (best) path
		# subtract a tile from each point in the path as long as that tile is a nav tile
		# if frame % 60 == 0:
		# for i in range(bpath.size()):
		# 	# var p = bpath[i]
		# 	var iio = 0
		# 	iio += 1
		# 	while tilemap.get_cellv(roundToN(bpath[i] + Vector2(0, gap), gap)/tilemap.cell_quadrant_size) == 0:
		# 		# print("changed %s to %s" % [bpath[i], bpath[i] + Vector2(0, gap)])
		# 		bpath[i] = roundToN(bpath[i] + Vector2(0, gap), gap)
		# 	bpath[i] = roundToN(bpath[i] - Vector2(0, gap), gap)
		# 	print(iio)
	
		for c in pre_player_data.keys():
			pre_player_data[c][1] -= 1
			if pre_player_data[c][1] <= 0:
				place_tile_local(tilemap, c, pre_player_data[c][0])
				if pre_player_data[c][2]:
					# places += 1
					place_label.text = "Places left: %s" % places
				else:
					# cuts += 1
					cut_label.text = "Cuts left: %s" % cuts
				pre_player_data.erase(c)
		
		# print(end_goal)
		update_closest_to_chara(target_positions)
		bpath = Navigation2DServer.map_get_path(chara.get_agent_rid(), chara.position, end_goal, false) # nav2d (best) 
		if bpath.size(): bpath.remove(0) # remove chara current position from path
		best_line.points = PoolVector2Array() # centre line in grid
		for i in range(bpath.size()):
			best_line.add_point(bpath[i])
		emit_signal("chara_can_move", bpath) # tell chara they can move
		# print(astar.get_point_path(0, 10))
		# print(astar.get_available_point_id())
		# move_along_path(chara.speed * delta)
		# update()
		frame += 1
	pass

func _physics_process(_delta):
	# _update_navigation_path(chara.position, end_goal)
	pass

func _input(event):
	if !game_over:
		if event.is_action_pressed("mouse_left"):
			# place tile
			if places > 0:
				pre_player_data[roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap)/tilemap.cell_quadrant_size] = [
					tilemap.get_cellv(roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap)/tilemap.cell_quadrant_size), # changed tile
					alteration_replacement_length, # how long before reverting change
					true # tile was placed
				]
				place_tile_global(tilemap, mouse_pos - Vector2(gap/2.0, gap/2.0), TILE_USER)
				places -= 1
				place_label.text = "Places left: %s" % places
		elif event.is_action_pressed("mouse_right"):
			# cut tile
			if cuts > 0:
				if tilemap.get_cellv(roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap)/tilemap.cell_quadrant_size) in [1, 2]:
					pre_player_data[roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap)/tilemap.cell_quadrant_size] = [
						# changed tile
						tilemap.get_cellv(roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap)/tilemap.cell_quadrant_size), 
						alteration_replacement_length/2.0, # how long before reverting change
						false # tile was cut
					]
					place_tile_global(tilemap, mouse_pos - Vector2(gap/2.0, gap/2.0), TILE_NAV)
					cuts -= 1
					cut_label.text = "Cuts left: %s" % cuts
		

func _draw():
	# for p in bpath:
	# 	draw_circle(p, 3, Color.white)
	pass

# place tile using local tilemap coordinates and return if it changed the tile
func place_tile_local(t, p: Vector2, id: int):
	# var q = t.get_cellv(p)
	t.set_cell(
		p.x,
		p.y,
		id
	)
	# return id != q

# place tile using global coordinates and return if it changed the tile
func place_tile_global(t, p: Vector2, id: int):
	# var q = t.get_cellv(p)
	t.set_cell(
		stepify(p.x, gap)/t.cell_quadrant_size,
		stepify(p.y, gap)/t.cell_quadrant_size,
		id
	)
	# return id != q

# initialise navigation2d search grid with a spawn location
func init_nav_grid(spawn: Vector2):
	for c in tilemap.get_used_cells():
		if tilemap.get_cell(c.x, c.y) in [0, 3]:
			# if tile is a valid tile or the target tile
			if tilemap.get_cell(c.x, c.y) == 3:
				# if the current tile is the target, save its id
				target_positions.append(c*tilemap.cell_quadrant_size)
				pass
			else:
				if c == spawn:
					chara.position = c*tilemap.cell_quadrant_size
				pass


# update astar search grid
# func update_astar_grid():
# 	astar.clear()
# 	for c in tilemap.get_used_cells():
# 		if tilemap.get_cell(c.x, c.y) in [0, 3]:
# 			# if tile is a valid tile or the target tile
# 			astar.add_point(id_count, c*tilemap.cell_quadrant_size, 0)
# 			if tilemap.get_cell(c.x, c.y) == 3:
# 				# if the current tile is the target, save its id
# 				end_goal = c*tilemap.cell_quadrant_size
# 				end_goal_id = id_count
# 			id_count = astar.get_available_point_id()
	
# 	for i in astar.get_points():
# 		for j in astar.get_points():
# 			# print(astar.get_point_position(j).distance_squared_to(astar.get_point_position(i)))
# 			if i != j and astar.get_point_position(j).distance_squared_to(astar.get_point_position(i)) <= gap*gap:
# 				astar.connect_points(i, j)

func roundToN(v: Vector2, n: int):
	return Vector2(stepify(v.x, n), stepify(v.y, n))

func get_time():
	return time


func update_closest_to_chara(vs: PoolVector2Array):
	var md = 10000000
	var cl = Vector2(100000, 100000)
	for p in vs:
		if p.distance_to(chara.position) == md:
			if abs(p.x - position.x) < abs(cl.x - position.x):
				md = p.distance_to(chara.position)
				cl = p
		elif p.distance_to(chara.position) < md:
			md = p.distance_to(chara.position)
			cl = p
	end_goal = cl


# kill chara
func chara_killed():
	game_over = true
	death_control.visible = true
	restart_button.disabled = false
	quit_button.disabled = false
	restart_button.visible = true
	quit_button.visible = true
	chara.queue_free()
	dtimer.stop()


func _on_Chara_reached_goal():
	place_tile_global(tilemap, end_goal, 1)
	retarget_positions.append(end_goal)
	target_positions.erase(end_goal)
	target_label.text = "Targets left: %s" % target_positions.size()
	if target_positions:
		cuts += 1
		places += 2
		end_goal = target_positions[0]
		# print("reached goal %s" % target_positions.size())
		dtimer.start(dtimer.time_left + target_positions.size())
	else:
		game_over = true
		win_control.visible = true
		restart_button.disabled = false
		quit_button.disabled = false
		restart_button.visible = true
		quit_button.visible = true
		print("reached all targets")
		
	
	
func _on_DeathTrigger_body_entered(_body:Node):
	chara_killed()


func _on_RestartButton_pressed():
	get_tree().reload_current_scene()


func _on_QuitButton_pressed():
	get_tree().quit()


func _on_DeathTimer_timeout():
	chara_killed()
