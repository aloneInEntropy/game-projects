extends Node2D


onready var tm = $TileMap
onready var ntm = $NormalTilemap
# onready var chara = $Chara

func _ready():
	visible = false
	# ntm.visible = false
	# load_tilemap(ntm)

# load a tilemap into the minimap to use
func load_tilemap(t: TileMap, chara: Node):
	tm.tile_set = t.tile_set
	chara.visible = false
	for c in t.get_used_cells():
		tm.set_cellv(c, t.get_cellv(c))

	var chara_pos = Sprite.new()
	chara_pos.texture = load("res://quad_128.png")
	chara_pos.position = tm.position + (chara.position * tm.scale)
	chara_pos.modulate = Color.green
	chara_pos.scale /= 16
	add_child(chara_pos)