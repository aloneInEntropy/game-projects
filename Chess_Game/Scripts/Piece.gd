extends TextureRect

onready var chessboard := get_parent().get_parent() # chessboard
onready var detection := $Detection # detection area
onready var rc := $RayCast2D # detection area
onready var pname := "" # name of piece

var bP_texture = preload("res://Assets/Chess Board/Pieces/Sprites/bP.png")
var wP_texture = preload("res://Assets/Chess Board/Pieces/Sprites/wP.png")
var bB_texture = preload("res://Assets/Chess Board/Pieces/Sprites/bB.png")
var wB_texture = preload("res://Assets/Chess Board/Pieces/Sprites/wB.png")
var bK_texture = preload("res://Assets/Chess Board/Pieces/Sprites/bK.png")
var wK_texture = preload("res://Assets/Chess Board/Pieces/Sprites/wK.png")
var bQ_texture = preload("res://Assets/Chess Board/Pieces/Sprites/bQ.png")
var wQ_texture = preload("res://Assets/Chess Board/Pieces/Sprites/wQ.png")
var bR_texture = preload("res://Assets/Chess Board/Pieces/Sprites/bR.png")
var wR_texture = preload("res://Assets/Chess Board/Pieces/Sprites/wR.png")
var bN_texture = preload("res://Assets/Chess Board/Pieces/Sprites/bN.png")
var wN_texture = preload("res://Assets/Chess Board/Pieces/Sprites/wN.png")

var position := Vector2() # 2d location of piece on chess chessboard, from 1 to 8
var dir : bool # is the piece travelling up (true) or down (false)?
var pattern := [Vector2(0, 0)] # locations this piece can travel to
var true_pattern := [] # locations this piece can travel to, ignoring any blocking pieces
var start_pawn_pattern = [Vector2(0, 1), Vector2(0, 2)]
var pawn_pattern = [Vector2(0, 1)]
var capture_pawn_pattern_1 = [Vector2(1, 1), Vector2(-1, 1)] # pieces traveling down
var capture_pawn_pattern_2 = [Vector2(1, -1), Vector2(-1, -1)] # pieces travelling up
var king_pattern = [Vector2(0, 1), Vector2(0, -1), Vector2(1, 0), Vector2(-1, 0), Vector2(1, 1), Vector2(1, -1), Vector2(-1, 1), Vector2(-1, -1)]
var knight_pattern = [Vector2(2, 1), Vector2(2, -1), Vector2(1, 2), Vector2(1, -2), Vector2(-1, 2), Vector2(-1, -2), Vector2(-2, 1), Vector2(-2, -1)]
var has_pawn_moved := false # if piece is a pawn, are they at their starting position?
var colour : bool # white = true, black = false
var held : bool # is this piece being held?
var turn : bool # is it this piece's (colour's) turn to move?

func _ready():
	held = false
	rc.add_exception(detection)
	

func set_piece(s: String):
	pname = s
	match s:
		"p":
			colour = false
			texture = bP_texture
		"P":
			colour = true
			texture = wP_texture
		"b":
			colour = false
			texture = bB_texture
		"B":
			colour = true
			texture = wB_texture
		"k":
			colour = false
			texture = bK_texture
		"K":
			colour = true
			texture = wK_texture
		"q":
			colour = false
			texture = bQ_texture
		"Q":
			colour = true
			texture = wQ_texture
		"r":
			colour = false
			texture = bR_texture
		"R":
			colour = true
			texture = wR_texture
		"n":
			colour = false
			texture = bN_texture
		"N":
			colour = true
			texture = wN_texture

	dir = colour
	update_pattern(chessboard.board)

# can this piece move to the position?
func can_move(to: Vector2):
	return turn and to in pattern

# move this piece to the position `to` on the chessboard, using chessboard coordinates
func move(to: Vector2):
	if can_move(to):
		position = to
		if pname == 'p' or pname == 'P':
			update_pattern(chessboard.board)
			has_pawn_moved = true
		return true
	return false

# update this piece's pattern
func update_pattern(b: Array):
	var tp = [[]]
	pattern = []
	true_pattern = []
	match pname:
		'n', 'N':
			tp[0] = knight_pattern
			true_pattern = tp.duplicate(true)
		'k', 'K':
			tp[0] = king_pattern
			true_pattern = tp.duplicate(true)
		'p', 'P':
			if !has_pawn_moved:
				if dir:
					for p in start_pawn_pattern:
						tp[0].append(Vector2(p.x, -p.y))
				else:
					tp[0] = start_pawn_pattern
			else:
				if dir:
					tp[0] = [Vector2(pawn_pattern[0].x, -pawn_pattern[0].y)]
				else:
					tp[0] = pawn_pattern
			if dir:
				tp[0].append_array(capture_pawn_pattern_2)
			else:
				tp[0].append_array(capture_pawn_pattern_1)
			true_pattern = tp.duplicate(true)
		'r', 'R':
			# vertical and horizontal
			tp = [[], [], [], []]
			for i in range(1, chessboard.tile_count):
				tp[0].append(Vector2(0, i))
				tp[1].append(Vector2(0, -i))
				tp[2].append(Vector2(i, 0))
				tp[3].append(Vector2(-i, 0))
			true_pattern = tp.duplicate(true)
		'b', 'B':
			# diagonal
			tp = [[], [], [], []]
			for i in range(1, chessboard.tile_count):
				tp[0].append(Vector2(i, i))
				tp[1].append(Vector2(-i, -i))
				tp[2].append(Vector2(i, -i))
				tp[3].append(Vector2(-i, i))
			true_pattern = tp.duplicate(true)
		'q', 'Q':
			# vertical, horizontal, and diagonal
			tp = [[], [], [], [], [], [], [], []]
			for i in range(1, chessboard.tile_count):
				tp[0].append(Vector2(0, i))
				tp[1].append(Vector2(0, -i))
				tp[2].append(Vector2(i, 0))
				tp[3].append(Vector2(-i, 0))
				tp[4].append(Vector2(i, i))
				tp[5].append(Vector2(-i, -i))
				tp[6].append(Vector2(i, -i))
				tp[7].append(Vector2(-i, i))
			true_pattern = tp.duplicate(true)
	

	var tpd = [] # values to remove from true pattern
	for i in range(len(true_pattern)):
		tpd.append([])
		for j in range(len(true_pattern[i])):
			true_pattern[i][j] += position
			if !point_on_chessboard(true_pattern[i][j]):
				tpd[i].append(true_pattern[i][j])
	
	for i in range(len(tpd)):
		for j in range(len(tpd[i])):
			true_pattern[i].erase(tpd[i][j])
			

	for i in range(len(tp)):
		var d = tp[i]
		for j in range(len(d)):
			var p = tp[i][j]
			p += position
			if point_on_chessboard(p):
				# if location is on the board
				var pos = b[p.y][p.x] 
				if pos:
					# + if there is a piece at this position
					if is_instance_valid(pos): 
						# + if the piece is not queued for destruction
						if pos.colour != colour: 
							# + if the piece is not the same colour as me
							if not p in pattern: 
								if pname == 'p' or pname == 'P':
									# + (if pawn) if in pawn capture group when added to position
									if p - position in capture_pawn_pattern_1 or p - position in capture_pawn_pattern_2:
										pattern.append(p) # add position :)
								else:
									pattern.append(p) # add position :)
						# if sliding piece or starting pawn
						if pname in ["r", "b", "q", "R", "B", "Q"] or ((pname == 'p' or pname == 'P') and !has_pawn_moved):
							break # add all empty squares up to the first piece, then change directions/ignore remaining squares
				else:
					# add if the point is otherwise on a blank square
					if not p in pattern:
						if pname == 'p' or pname == 'P':
							# + (if pawn) if NOT in pawn capture group when added to position 
							if not (p - position in capture_pawn_pattern_1) and not (p - position in capture_pawn_pattern_2):
								pattern.append(p)
						else:
							pattern.append(p)
			
func point_on_chessboard(p: Vector2) -> bool:
	return p.x < chessboard.tile_count and p.x >= 0 and p.y < chessboard.tile_count and p.y >= 0
