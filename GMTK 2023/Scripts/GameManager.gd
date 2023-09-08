# This script contains auxiliary functions, algorithms, and variables to help avoid cluttering.

extends Node

onready var bg_audio = $BGAudio
onready var jump_audio = $JumpAudio
onready var place_audio = $PlaceAudio
onready var cut_audio = $CutAudio
onready var goal_audio = $GoalAudio
onready var death_audio = $DeathAudio
onready var win_audio = $WinAudio
onready var action_audio := $ActionAudio

# onready var _bus := AudioServer.get_bus_index(audio_bus_name)
onready var gap := 32

var level_playing := -1 # the index of the level currently playing
var levels_completed := [
	false, false, false, false, false, false, false, false, false, false, false
]

var REFILL_INF := 10000

var TILE_SOLID_MODULATE := Color("#936161")
var TILE_NAV_MODULATE := Color("#00186c")

var TILE_NAV := 0 			# navigation tile
var TILE_OTHER := 1 		# other (collision) tile
var TILE_SOLID := 2 		# ground/wall tile
var TILE_TARGET := 3 		# target tile
var TILE_USER := 4 			# user-placed tile

func _ready():
	# get_tree().get_nodes_in_group("audio")
	bg_audio.play()
	pass

func _process(_delta):
	# print(level_playing)
	# print(Engine.get_frames_per_second())
	pass

# recenter a point on a rect
func recenter_vector(v: Vector2, val: int) -> Vector2:
	return v - single_vector(val/2.0)

# convert a value into a vector on both axes
func single_vector(val) -> Vector2:
	return Vector2(val, val)

# convert a global coordinate to a tilemap coordinate
func global_to_tilemap_coord(t: TileMap, p: Vector2) -> Vector2:
	return roundToN(p - t.global_transform.origin, float(gap * t.scale.x))/(t.cell_quadrant_size * t.scale.x)

# convert a tilemap coordinate to a global coordinate
func tilemap_to_global_coord(t: TileMap, p: Vector2) -> Vector2:
	return (p - t.global_transform.origin)*t.cell_quadrant_size

# place tile using local tilemap coordinates
func place_tile_local(t: TileMap, p: Vector2, id: int):
	t.set_cellv(p, id)

# place tile using global coordinates
func place_tile_global(t: TileMap, p: Vector2, id: int):
	t.set_cellv(global_to_tilemap_coord(t, p), id)

func roundToN(v: Vector2, n) -> Vector2:
	return Vector2(stepify(v.x, n), stepify(v.y, n))

func set_master_volume(vol: int):
	for a in get_tree().get_nodes_in_group("sound"):
		a.volume_db = vol
		# get_tree().scene

# play the `lvl`th level. level 0 is the tutorial
func play_level(lvl: int):
	level_playing = lvl
	if lvl == 0:
		get_tree().change_scene("res://Scenes/Tutorial.tscn")
	else:
		print("################################ %s ################################" % level_playing)
		print(get_tree().change_scene("res://Scenes/Stage%s.tscn" % lvl))

# update the `lvl`th level status with whether or not the player beat the level
func update_level_state(level_won: bool, lvl := level_playing):
	print(level_playing)
	levels_completed[lvl] = level_won

# restart the current level
func restart_current_level():
	if win_audio.playing:
		win_audio.stop()
	print(get_tree().reload_current_scene())

# play a UI action sound. the default sound can be found at Assets/Audio/select.wav
func play_action_audio(audio_path := "res://Assets/Audio/select.wav"):
	action_audio.stream = load(audio_path)
	action_audio.play()
	
# play the game win sound. the default sound can be found at Assets/Audio/game_won.wav
func play_win_audio(audio_path := "res://Assets/Audio/game_won.wav"):
	win_audio.stream = load(audio_path)
	win_audio.play()

# play the death sound. the default sound can be found at Assets/Audio/death.wav
func play_death_audio(audio_path := "res://Assets/Audio/death.wav"):
	death_audio.stream = load(audio_path)
	death_audio.play()

# play the cut sound. the default sound can be found at Assets/Audio/cut.wav
func play_cut_audio(audio_path := "res://Assets/Audio/cut.wav"):
	cut_audio.stream = load(audio_path)
	cut_audio.play()

# play the place sound. the default sound can be found at Assets/Audio/place.wav
func play_place_audio(audio_path := "res://Assets/Audio/place.wav"):
	place_audio.stream = load(audio_path)
	place_audio.play()

# play the jump sound. the default sound can be found at Assets/Audio/jump.wav
func play_jump_audio(audio_path := "res://Assets/Audio/jump.wav"):
	jump_audio.stream = load(audio_path)
	jump_audio.play()

# play the goal sound. the default sound can be found at Assets/Audio/goal.wav
func play_goal_audio(audio_path := "res://Assets/Audio/goal.wav"):
	goal_audio.stream = load(audio_path)
	goal_audio.play()



