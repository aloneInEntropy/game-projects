extends CanvasLayer

onready var stage := get_parent()
onready var label := $Label
onready var place_label := $PlaceLabel
onready var cut_label := $CutLabel
onready var target_label := $TargetLabel
onready var time_label := $TimeLabel
onready var death_control := $DeathControl
onready var win_control := $WinControl
onready var pause_control := $PauseControl
# onready var settings_control := $SettingsControl
# onready var main_audio_slider := $SettingsControl/HSlider
onready var darken_bg := $DarkenBG
onready var restart_button := $RestartButton
onready var quit_button := $QuitButton
onready var pause_button := $PauseButton
onready var levels_button := $LevelButton
onready var mmap := $Minimap
onready var bline := $Minimap/Line2D
onready var crfill := $CutRefill
onready var prfill := $PlaceRefill

var LINE_VALID_COLOUR := Color("#72ee7a")
var LINE_INVALID_COLOUR := Color("#fd5300")

var settings := preload("res://Scenes/Settings.tscn")
var settings_open := false
var gset : Node

var pause_screens := [] # the stack of screens visited since the game was paused. only when this array is empty will gameplay resume.

signal pause_toggled

func _ready():
	restart_button.disabled = true
	quit_button.disabled = true
	levels_button.disabled = true
	restart_button.visible = false
	quit_button.visible = false
	levels_button.visible = false
	death_control.visible = false
	win_control.visible = false
	pause_control.visible = false
	# settings_control.visible = false
	# main_audio_slider.editable = false
	darken_bg.environment.adjustment_enabled = false
	mmap.visible = false
	# bline.visible = false
	# main_audio_slider.value = db2linear(AudioServer.get_bus_volume_db(
	

func _process(_delta):
	if settings_open and !is_instance_valid(gset):
		settings_open = false

# load a tilemap into the minimap to use.
# `t` is the tilemap to load
# `prim` is the primary node to highlight
# `line` is the best line to follow
# `tsize` is the size (width and height) of the tilemap `t`
# `vis` checks whether or not to show the minimap
func load_minimap(t: TileMap, prim: Node, line_pts: Line2D, vis := true):
	mmap.visible = vis
	# mmap = TileMap.new()
	# mmap.
	mmap.tile_set = TileSet.new()

	for c in mmap.get_used_cells():
		mmap.set_cellv(c, -1)

	# copy all textures into minimap tilemap if not already present
	for id in t.tile_set.get_tiles_ids():
		# if not id in mmap.tile_set.get_tiles_ids():
			mmap.tile_set.create_tile(id)
			mmap.tile_set.tile_set_texture(id, t.tile_set.tile_get_texture(id))
			mmap.tile_set.tile_set_modulate(id, t.tile_set.tile_get_modulate(id))
	
	# add chara tile
	if not t.tile_set.get_tiles_ids().size() in mmap.tile_set.get_tiles_ids():
		mmap.tile_set.create_tile(t.tile_set.get_tiles_ids().size())
		# mmap.tile_set.tile_set_texture(t.tile_set.get_tiles_ids().size(), load("res://Assets/Tiles/valid.png"))
		mmap.tile_set.tile_set_texture(t.tile_set.get_tiles_ids().size(), load("res://Assets/Sprites/Chara.png"))

	# copy tiles and add chara
	var maxx = -1000000000
	var maxy = -1000000000
	var minx = 1000000000
	var miny = 1000000000
	for c in t.get_used_cells():
		maxx = max(c.x, maxx)
		maxy = max(c.y, maxy)
		minx = min(c.x, minx)
		miny = min(c.y, miny)
		mmap.set_cellv(c - t.get_used_cells()[0], t.get_cellv(c))
		if c == GM.global_to_tilemap_coord(t, GM.roundToN(prim.position, GM.gap)):
			mmap.set_cellv((c - Vector2.ONE - t.get_used_cells()[0]).abs(), t.tile_set.get_tiles_ids().size())
			# print(c)
			bline.position = c - Vector2(GM.gap, 0)

	var tw = maxx - minx
	var th = maxy - miny

	# fit the minimap to a position with equal distance from the left and right sides of the screen, and below the label GUI background
	mmap.position = (((prim.get_viewport().size / 2)) - \
					((Vector2(tw, th) / 2) * mmap.cell_quadrant_size * mmap.scale)).abs() + \
					Vector2(0, get_node("GUIBg").rect_size.y/2)
	
	# print((prim.get_viewport().size / 2) - tsize)
	# print(prim.get_viewport().size)
	# print((prim.get_viewport().size - tsize) / 2)
	
	# print(prim.get_viewport().size)
	# print(mmap.position)

	bline.default_color = LINE_VALID_COLOUR if stage.path_is_valid else LINE_INVALID_COLOUR
	
	bline.points = line_pts.points
	for p in range(len(bline.points)):
		bline.points[p] *= mmap.scale
		bline.points[p] -= (mmap.scale * Vector2(0, GM.gap/2.0))
		bline.points[p] -= GM.tilemap_to_global_coord(t, t.get_used_cells()[0]) * mmap.scale


func _unhandled_input(event):
	if !stage.game_playing:
		if event is InputEventMouseButton and event.doubleclick:
		# if event.is_action_pressed("mouse_left"):
			var tpos := GM.global_to_tilemap_coord(mmap, GM.recenter_vector(event.position, GM.gap * mmap.scale.x))
			print(mmap.get_cellv(tpos))
			if mmap.get_cellv(tpos) == GM.TILE_TARGET:
				stage.update_chosen_target(stage.tilemap, tpos)
				stage.open_minimap(false)
			print(tpos)
			print(tpos in mmap.get_used_cells())

func _on_Stage_game_over(win: bool):
	pause_button.disabled = true
	restart_button.visible = true
	restart_button.disabled = false
	quit_button.visible = true
	quit_button.disabled = false
	levels_button.visible = true
	levels_button.disabled = false
	if win:
		win_control.visible = true
	else:
		death_control.visible = true

func _on_Stage_update_cuts(val: int):
	cut_label.text = "Cuts: %s" % val

func _on_Stage_update_places(val: int):
	place_label.text = "Places: %s" % val

func _on_Stage_update_timer(val: float):
	time_label.text = "Time: %.2f" % val

func _on_Stage_update_targets(val: int):
	target_label.text = "Targets left: %s" % val

func _on_RestartButton_pressed():
	GM.restart_current_level()

func _on_QuitButton_pressed():
	get_tree().quit()

func _on_LevelButton_pressed():
	if GM.win_audio.playing:
		GM.win_audio.stop()
	print(get_tree().change_scene("res://Scenes/Levels.tscn"))

func _on_Stage_pause_toggled(show_control: bool):
	if show_control:
		pause_control.visible = true
	else:
		pause_control.visible = false
	darken_bg.environment.adjustment_enabled = not darken_bg.environment.adjustment_enabled
	GM.play_action_audio()

func _on_PauseButton_pressed():
	emit_signal("pause_toggled")
	GM.play_action_audio()
	

func _on_Resume_gui_input(event:InputEvent):
	if event.is_action_pressed("mouse_left"):
		GM.play_action_audio()
		emit_signal("pause_toggled")

func _on_Resume_mouse_entered():
	$PauseControl/Resume.bbcode_text = "[center][color=#aaaaff]Resume"
	
func _on_Resume_mouse_exited():
	$PauseControl/Resume.bbcode_text = "[center][color=white]Resume"


func _on_Restart_gui_input(event:InputEvent):
	if event.is_action_pressed("mouse_left"):
		GM.play_action_audio()
		print(get_tree().reload_current_scene())

func _on_Restart_mouse_entered():
	$PauseControl/Restart.bbcode_text = "[center][color=#aaaaff]Restart"

func _on_Restart_mouse_exited():
	$PauseControl/Restart.bbcode_text = "[center][color=white]Restart"
	

func _on_Minimap_gui_input(event):
	if event.is_action_pressed("mouse_left"):
		pause_control.visible = false
		stage.open_minimap(false)

func _on_Minimap_mouse_entered():
	$PauseControl/Minimap.bbcode_text = "[center][color=#aaaaff]Minimap"

func _on_Minimap_mouse_exited():
	$PauseControl/Minimap.bbcode_text = "[center][color=white]Minimap"
	

func _on_Level_Select_gui_input(event:InputEvent):
	if event.is_action_pressed("mouse_left"):
		GM.play_action_audio()
		print(get_tree().change_scene("res://Scenes/Levels.tscn"))

func _on_LevelSelect_mouse_entered():
	$PauseControl/LevelSelect.bbcode_text = "[center][color=#aaaaff]Level Select"

func _on_LevelSelect_mouse_exited():
	$PauseControl/LevelSelect.bbcode_text = "[center][color=white]Level Select"
		

func _on_Settings_gui_input(event:InputEvent):
	# print("Not implemented yet!")
	if event.is_action_pressed("mouse_left"):
		GM.play_action_audio()
		settings_open = true
		gset = settings.instance()
		add_child(gset)

func _on_Settings_mouse_entered():
	$PauseControl/Settings.bbcode_text = "[center][color=#aaaaff]Settings"

func _on_Settings_mouse_exited():
	$PauseControl/Settings.bbcode_text = "[center][color=white]Settings"
		

func _on_Exit_gui_input(event:InputEvent):
	if event.is_action_pressed("mouse_left"):
		get_tree().quit()

func _on_Exit_mouse_entered():
	$PauseControl/Exit.bbcode_text = "[center][color=#aaaaff]Exit"

func _on_Exit_mouse_exited():
	$PauseControl/Exit.bbcode_text = "[center][color=white]Exit"


func return_from_screen():
	if settings_open:
		gset.queue_free()
	else:
		emit_signal("pause_toggled")


func _on_Stage_update_refill(cval: float, show_c: bool, pval: float, show_p: bool):
	crfill.visible = show_c
	crfill.value = cval * 100
	# print(cval)
	# print(pval)

	prfill.visible = show_p
	prfill.value = pval * 100
	