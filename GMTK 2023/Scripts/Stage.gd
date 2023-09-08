extends Node2D

onready var tilemap : TileMap = $TileMap
onready var chara : KinematicBody2D = $Chara
onready var best_line : Line2D = $Line2D
onready var dtimer : Timer = $DeathTimer
onready var gui : CanvasLayer = $GUI
onready var camera : Camera2D = $Camera2D
onready var camera_follow : RemoteTransform2D = $Chara/RemoteTransform2D
onready var normal_target_sprite : Sprite = $NormalTarget
onready var user_target_sprite : Sprite = $UserTarget
onready var cursor_tile : Sprite = $CursorTile

var TILE_NAV := 0 			# navigation tile
var TILE_OTHER := 1 		# other (collision) tile
var TILE_SOLID := 2 		# ground/wall tile
var TILE_TARGET := 3 		# target tile
var TILE_USER := 4 			# user-placed tile

var LINE_VALID_COLOUR := Color("#6680ff")
var LINE_INVALID_COLOUR := Color("#ff3b3f")

var DOUBLE_CLICK_DELTA := 10 # maximum frames between clicks to register double click

onready var pre_player_data = {} # data regarding tiles before the player has interacted with them
onready var target_positions := [] # global vector2 locations of targets
onready var retarget_positions := [] # global vector2 locations of collected targets 

var astar := AStar2D.new()
var rng := RandomNumberGenerator.new()
var can_pause := true
var is_paused := false
var can_move_cam := true
var id_count := 0
var gap := 32
var cam_timer_start := 1
var cam_timer = cam_timer_start
var tile_anim_timer_start := 30
var tile_anim_timer = tile_anim_timer_start
var mouse_pos : Vector2 # mosue position
var mouse_snapshot_pos : Vector2 # mouse position the moment it is pressed
var end_goal : Vector2 # the current chara target
var end_goal_id : int
var move_object_held := -1 # id of moveable object held (-1 if none held)
var map
var bpath := PoolVector2Array()
var frame := 0
var time := 0.0
var double_click_start := -100
var path_is_valid := true # is the current path towards the end goal valid (i.e., reachable?)
var is_target_chosen := false # has the user chosen a target to move towards
var target_chosen : Vector2 # the target the user chose to move towards
var tilemap_width : int # how many tiles are on the x-axis?
var tilemap_height : int # how many tiles are on the y-axis?

export var game_playing := true
export var randomise_start_pos := false 			# should the chara have a random start position?
export var alteration_replacement_length := 300 	# how long until a player alteration is reverted? (in frames)
export var places := 5 								# how many tiles can the player place?
export var extra_place_timer := 10000 				# how many seconds need to pass before gaining an extra placement?
export var cuts := 5 								# how many tiles can the player remove?
export var extra_cut_timer := 10000 				# how many seconds need to pass before gaining an extra cut?
export var do_targets_respawn := true 				# do collected targets respawn after an amount of time?
export var target_respawn_time := 60 				# how long it takes before targets respawn, in seconds

signal pause_toggled 								# pause the game
signal chara_can_move(mpath) 						# send the chara the path it can move to
signal game_over(win) 								# tell the GUI whether or not the player beat the stage
signal update_cuts(val) 							# update the value of the cuts field on the GUI
signal update_places(val) 							# update the value of the places field on the GUI
signal update_timer(val) 							# update the value of the timer field on the GUI
signal update_targets(val) 							# update the value of the target field on the GUI
signal update_refill(cval, show_c, pval, show_p)	# update the cut and placement refill fields on the GUI

func _ready():
	gui.visible = true
	rng.randomize()
	tilemap_width = int(tilemap.get_used_rect().size.x)
	tilemap_height = int(tilemap.get_used_rect().size.y)
	var rnd_x := rng.randi_range(1, (tilemap_width/int(gap)) - 1)
	var rnd_y := rng.randi_range(1, (tilemap_height/int(gap)) - 1)

	# camera.limit_top = int(tilemap.position.y)
	# camera.limit_bottom = int(tilemap.position.y) + ((tilemap_height + 1 )* gap)
	# camera.limit_left = int(tilemap.position.x)
	# camera.limit_right = int(tilemap.position.x) + ((tilemap_width + 1) * gap)
	
	print(camera.limit_top)
	print(camera.limit_bottom)
	print(camera.limit_left)
	print(camera.limit_right)
	
	if randomise_start_pos:
		init_nav_grid(Vector2(rnd_x, rnd_y))
	else:
		init_nav_grid()
	end_goal = target_positions[0]
	update_cuts(cuts)
	update_places(places)
	update_targets(0)
	dtimer.start(dtimer.wait_time)
	camera.position = chara.position
	for animator in get_tree().get_nodes_in_group("tile_anim"):
		animator.play()

	
func _process(delta):
	# print(mouse_pos)
	# print(camera.current)
	# print(target_positions)
	cursor_tile.position = GM.roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap) 
	is_paused = !game_playing and can_pause

	if game_playing:
		frame += 1
		time += delta
		emit_signal("update_timer", dtimer.time_left)
		mouse_pos = get_global_mouse_position()

		cam_timer = clamp(cam_timer - 1, -1, cam_timer_start)

		# temporary death barrier, first 5 seconds
		if frame == (60*5):
			for c in tilemap.get_used_cells():
				if c.y >= 60 and tilemap.get_cell(c.x, c.y) == TILE_OTHER:
					GM.place_tile_local(tilemap, c, 0)
					# end_game(true)
					
		# every 60 seconds
		if frame % (60*60) == 0:
			if do_targets_respawn and !retarget_positions.empty():
				update_targets(1)
		
		update_refill(
			(frame % extra_cut_timer) / float(extra_cut_timer), 
			extra_cut_timer < GM.REFILL_INF, 
			(frame % extra_place_timer) / float(extra_place_timer), 
			extra_place_timer < GM.REFILL_INF
		)
		
		# every x frames
		if frame % extra_cut_timer == 0:
			update_cuts(cuts + 1)
		if frame % extra_place_timer == 0:
			update_places(places + 1)

		for c in pre_player_data.keys():
			pre_player_data[c][1] = clamp(pre_player_data[c][1] - 1, -10, alteration_replacement_length)
			# replace tiles only if the chara is not in them
			if pre_player_data[c][1] <= 0 and GM.roundToN(chara.position, gap) != c:
				GM.place_tile_local(tilemap, c, pre_player_data[c][0])
				if pre_player_data[c][2]:
					# revert place
					# update_places(places)
					pass
				else:
					# revert cut
					# update_cuts(cuts)
					pass
				pre_player_data.erase(c)
		
		# print(end_goal)
		best_line.points = PoolVector2Array() # centre line in grid
		update_closest_to_chara(target_positions)
		bpath = Navigation2DServer.map_get_path(chara.get_agent_rid(), chara.position, end_goal, false) # nav2d (best) 
		# if end_goal in bpath:
		if bpath.size(): 
			bpath.remove(0) # remove chara current position from path
			best_line.points = bpath
			best_line.points[best_line.points.size() - 1] = end_goal + GM.single_vector(gap/2.0)
		# for i in range(bpath.size()):
		# 	# bpath[i] += GM.single_vector(gap/2.0)
		# 	best_line.add_point(bpath[i])
		emit_signal("chara_can_move", bpath) # tell chara they can move

		best_line.default_color = LINE_VALID_COLOUR if path_is_valid else LINE_INVALID_COLOUR
		update()
	pass


func _input(event):
	if game_playing:
		if event.is_action_pressed("mouse_left"):
			if !Input.is_action_pressed("cam_drag"):
				if event.doubleclick:
					user_place_tile(mouse_pos, true)
				else:
					user_place_tile()
		elif event.is_action_pressed("mouse_right"):
			user_cut_tile()
		elif event is InputEventMouseMotion:
			# move camera
			if can_move_cam and event.button_mask == BUTTON_MASK_MIDDLE or (event.button_mask == BUTTON_MASK_LEFT and Input.is_action_pressed("cam_drag")):
				camera.smoothing_enabled = false
				camera.position -= event.relative
			elif event.button_mask == BUTTON_MASK_LEFT:
				user_place_tile()
			elif event.button_mask == BUTTON_MASK_RIGHT:
				user_cut_tile()

	cursor_tile.visible = !event.is_action_pressed("cam_drag")

	if event.is_action_pressed("minimap") and can_pause:
		open_minimap(true)

	if event.is_action_pressed("ui_pause") and can_pause:
		if !gui.settings_open:
			toggle_pause()
		else:
			gui.return_from_screen()
		
	if event.is_action_pressed("debug_restart"):
		GM.restart_current_level()
		
	if event.is_action_pressed("toggle_camera"):
		if is_instance_valid(chara) and can_move_cam:
			camera.smoothing_enabled = true
			camera.position = chara.position
	

func _draw():
	# for p in bpath:
	# 	draw_circle(p, 3, Color.white)
	# draw_circle(gui.mmap.position, 3, Color.white)
	pass


# initialise navigation2d search grid with a spawn location in tilemap coordinates
func init_nav_grid(spawn := chara.position):
	var maxx = -1000000000
	var maxy = -1000000000
	var minx = 1000000000
	var miny = 1000000000
	for tm in get_tree().get_nodes_in_group("tilemap"):
		for c in tm.get_used_cells():
			maxx = max(c.x, maxx)
			maxy = max(c.y, maxy)
			minx = min(c.x, minx)
			miny = min(c.y, miny)
			if tm.get_cellv(c) in [TILE_NAV, TILE_TARGET]:
				# if tile is a valid tile or the target tile
				if tm.get_cellv(c) == TILE_TARGET:
					# if the current tile is the target, save its id
					target_positions.append(GM.tilemap_to_global_coord(tm, c))
					pass
				else:
					if tm.get_cellv(spawn) == TILE_NAV:
						if c == spawn:
							chara.position = GM.tilemap_to_global_coord(tm, c)
					else:
						# expand spawn options in a circle around the initial spawn position until a position is valid
						for i in range(1, tilemap_height + tilemap_width):
							if tm.get_cellv(spawn + Vector2(0, i)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(0, i))
								break
							elif tm.get_cellv(spawn + Vector2(0, -i)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(0, -i))
								break
							elif tm.get_cellv(spawn + Vector2(i, 0)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(i, 0))
								break
							elif tm.get_cellv(spawn + Vector2(-i, 0)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(-i, 0))
								break
							elif tm.get_cellv(spawn + Vector2(i, i)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(i, i))
								break
							elif tm.get_cellv(spawn + Vector2(-i, -i)) == TILE_NAV:
								chara.position =GM.tilemap_to_global_coord(tm,  spawn + Vector2(-i, -i))
								break
							elif tm.get_cellv(spawn + Vector2(i, -i)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(i, -i))
								break
							elif tm.get_cellv(spawn + Vector2(-i, i)) == TILE_NAV:
								chara.position = GM.tilemap_to_global_coord(tm, spawn + Vector2(-i, i))
								break
		# print("%s | %s" % [Vector2(maxx, maxy), Vector2(minx, miny)])
		if tm.visible:
			tilemap_width = maxx - minx
			tilemap_height = maxy - miny


# place a tile at a specific position, usually the player cursor
func user_place_tile(pos := mouse_pos, dc := false):
	if get_viewport().get_mouse_position().y > gap:
		# ignore all clicks on the GUI
		var tpos := GM.global_to_tilemap_coord(tilemap, GM.recenter_vector(pos, gap))
		if places > 0:
			# place tile
			if not tilemap.get_cellv(tpos) in [TILE_USER, TILE_TARGET]:
				pre_player_data[tpos] = [
					tilemap.get_cellv(tpos), # changed tile
					alteration_replacement_length, # how long before reverting change
					true # tile was placed
				]
				GM.place_tile_global(tilemap, GM.recenter_vector(pos, gap), TILE_USER)
				update_places(places - 1)
				GM.play_place_audio()
		if tilemap.get_cellv(tpos) == TILE_TARGET and dc:
			update_chosen_target(tilemap, tpos)
			# register double click
			# if abs(frame - double_click_start) <= DOUBLE_CLICK_DELTA:
				# disable chosen target only if same target is is double clicked twice
					# print("current target: %s" % target_chosen) if is_target_chosen else print("not current target: %s" % target_chosen)
			double_click_start = frame


# destroy a tile at a specific position, usually the player cursor
func user_cut_tile(pos := mouse_pos):
	if get_viewport().get_mouse_position().y > gap:
		# ignore all clicks on the GUI
		if cuts > 0:
			# cut tile
			var tpos := GM.global_to_tilemap_coord(tilemap, GM.recenter_vector(pos, gap))
			# if tilemap.get_cellv(roundToN(mouse_pos - Vector2(gap/2.0, gap/2.0), gap)/tilemap.cell_quadrant_size) in [1, 2]:
			if tilemap.get_cellv(tpos) in [TILE_SOLID, TILE_OTHER]:
				pre_player_data[tpos] = [
					# changed tile
					tilemap.get_cellv(tpos), 
					alteration_replacement_length/2.0, # how long before reverting change
					false # tile was cut
				]
				GM.place_tile_global(tilemap, GM.recenter_vector(pos, gap), TILE_NAV)
				update_cuts(cuts - 1)
				GM.play_cut_audio()


# update the chosen target.
# `t` is the tilemap in question
# `p` is the chosen target
func update_chosen_target(t: TileMap, p: Vector2):
	if is_target_chosen and target_chosen == GM.tilemap_to_global_coord(t, p):
		is_target_chosen = false
	else:
		target_chosen = GM.tilemap_to_global_coord(t, p)
		is_target_chosen = true
		best_line.points = PoolVector2Array() # centre line in grid
		update_closest_to_chara(target_positions)
		bpath = Navigation2DServer.map_get_path(chara.get_agent_rid(), chara.position, end_goal, false) # nav2d (best) 
		# if end_goal in bpath:
		if bpath.size(): 
			bpath.remove(0) # remove chara current position from path
			best_line.points = bpath
			best_line.points[-1] = end_goal + GM.single_vector(gap/2.0)

func get_time():
	return time


# update the amount of cuts the player has left
func update_cuts(val: int):
	cuts = val
	emit_signal("update_cuts", val)
	

# update the amount of tile placements the player has left
func update_places(val: int):
	places = val
	emit_signal("update_places", val)
	

# update the amount of time the player has left
func update_timer(val: float):
	dtimer.start(val)
	emit_signal("update_timer", val)


# update the cut and place refil
func update_refill(cval: float, show_c: bool, pval: float, show_p: bool):
	emit_signal("update_refill", cval, show_c, pval, show_p)
	

# update the amount of targets the player has left to collect. 
# if `adding` is > 0, add a target to the target array. if `take_from_retarget` is true, ignore `val` and take the target from the top of the queue and add it to the target array. the value of `take_from_retarget` is irrelevant if `adding` is non-positive. 
# if `adding` is < 0, remove the collected target
# if `adding` is 0, update the GUI and do nothing else.
func update_targets(adding: int, val := end_goal, take_from_retarget := true):
	if adding > 0:
		if take_from_retarget:
			target_positions.append(retarget_positions[0])
			GM.place_tile_global(tilemap, retarget_positions.pop_front(), TILE_TARGET)
		else:
			target_positions.append(val)
			GM.place_tile_global(tilemap, val, TILE_TARGET)
	elif adding < 0:
		target_positions.erase(val)
		retarget_positions.append(val)
		GM.place_tile_global(tilemap, end_goal, TILE_OTHER)
	emit_signal("update_targets", target_positions.size())


# update the end goal, which is the closest target to the chara. this will be the closest target with a valid path or, barring that, the closest target regardless.
# `vs` is the set of target points as an array of Vector2 objects
func update_closest_to_chara(vs: PoolVector2Array):
	var md = 10000000
	var cl = Vector2(1000000, 1000000)
	if is_target_chosen:
		# if the player chose a target, move towards that one regardless of proximity to other targets
		end_goal = target_chosen
		user_target_sprite.visible = true
		user_target_sprite.position = end_goal + GM.single_vector(gap/2.0)
	else:
		user_target_sprite.visible = false
		var valid_closest = {} # dict of {targets: reachability}
		for p in vs:
			# for each target, check if the goal is in the path towards it. if it isn't, there is no valid path.
			bpath = Navigation2DServer.map_get_path(chara.get_agent_rid(), chara.position, p, false) # nav2d (best)
			valid_closest[p] = p in bpath
			# if p in bpath:
			# 	valid_closest.append(true)
			# else:
			# 	valid_closest.append(false)
		
		if true in valid_closest.values():
			# if there are any valid paths, move towards the closest of them.
			for p in valid_closest:
				if valid_closest[p]:
					if p.distance_to(chara.position) == md:
						# prioritise vertical proximity when distances are the same
						if abs(p.y - position.y) < abs(cl.y - position.y):
							md = p.distance_to(chara.position)
							cl = p
					elif p.distance_to(chara.position) < md:
						md = p.distance_to(chara.position)
						cl = p
		else:
			# if there are no valid paths, move towards the closest target.
			for i in range(len(vs)):
				var p := vs[i]
				if p.distance_to(chara.position) < md:
					md = p.distance_to(chara.position)
					cl = p

		end_goal = cl
		normal_target_sprite.position = end_goal + GM.single_vector(gap/2.0)
	
	bpath = Navigation2DServer.map_get_path(chara.get_agent_rid(), chara.position, end_goal, false) # nav2d (best) 
	path_is_valid = end_goal in bpath

# toggle the game pause.
# if paused, the game stops all chara movement, pathfinding, and stage interaction and opens the GUI pause menu
# if `show_control` is false, it hides the actual pause menu and shows the minimap
func toggle_pause(show_control := true):
	game_playing = not game_playing
	dtimer.paused = not dtimer.paused
	if game_playing:
		gui.mmap.visible = false
		emit_signal("pause_toggled", false)
		for animator in get_tree().get_nodes_in_group("tile_anim"):
			animator.playback_speed = 1
	else:
		for animator in get_tree().get_nodes_in_group("tile_anim"):
			animator.playback_speed = 0
		emit_signal("pause_toggled", show_control)

# open the GUI minimap. 
# `p` decides whether or not to toggle pause
# ---
# `t` is the default tilemap
# `c` is the focus node,
# `b` is the (best) line, a Line2D node,
# `v` is the visibility of the minimap
func open_minimap(p : bool, t := tilemap, c = chara, b := best_line, v := true):
	gui.load_minimap(t, c, b, v)
	if p:
		toggle_pause(false)

# end the game, with a bool argument specifiying if it won or not
func end_game(chara_won: bool):
	game_playing = false
	can_pause = false
	chara.queue_free()
	dtimer.stop()
	if chara_won:
		GM.play_win_audio()
		GM.update_level_state(true)
	else:
		GM.play_death_audio()
	emit_signal("game_over", chara_won)

func _on_Chara_reached_goal():
	print("reached target at %s" % end_goal)
	update_targets(-1)
	if is_target_chosen and end_goal == target_chosen:
		is_target_chosen = false
	if !target_positions.empty():
		end_goal = target_positions[0]
		update_cuts(cuts + 1)
		update_places(places + 1)
		update_timer(dtimer.time_left + target_positions.size())
	else:
		print("reached all targets")
		end_game(true)
		
	
func _on_DeathTrigger_body_entered(body:Node):
	if body == chara:
		end_game(false)

func _on_DeathTimer_timeout():
	end_game(false)

func _on_GUI_pause_toggled():
	toggle_pause()
