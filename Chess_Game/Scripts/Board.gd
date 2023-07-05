extends Control

onready var chessboard = $ChessBoard
onready var chessboard_label = $ChessBoardLabels
onready var bg = $Background
onready var hoverbox = $HoverBox
onready var moving_piece_layer = $MovingPiece
onready var piece_layer = $Pieces
onready var gui_banner = $GUIBanner
onready var label = $GUIBanner/Label
onready var turn_label = $GUIBanner/Turn
onready var check_label = $GUIBanner/KingCheck
onready var moves_label = $GUIBanner/MovesLabel
onready var meta_banner = $MetaBanner
onready var win_banner = $MetaBanner/WinBanner
onready var win_banner_text = $MetaBanner/WinBanner/Win
onready var restart_button = $MetaBanner/WinBanner/RestartButton
onready var quit_button = $MetaBanner/WinBanner/QuitButton

var board_side # length of board side
var board_pos # global location of chessboard
var box_side # length of box side
var norm_scale : Vector2 # normalisation scale to scale textures by to match chessboard size
var tile_count := 8 # number of tiles on chess board (usually 8)
var box : Vector2 # template for each chessboard square
var mouse_pos : Vector2 # mouse position
var tiled_mouse_pos : Vector2 # mouse position rounded to nearest square
var picked_up := false # is a piece picked up?
var prev_picked_pos : Vector2 # initial square player clicked on
var mouse_click_delay := 10 # delay before player can click again
var mouse_click_delay_rate := mouse_click_delay
var player_colour : bool # white = true, black = false
var moves = [] # array of moves performed by players
var turns := 0 # number of turns played (1 turn = 1 white + 1 black move)
var ds = [] # available pattern shower
var cds = [] # check pattern shower
var white_pieces = []
var black_pieces = []
var fen = ""
var can_start := true

var piece_instance = preload("res://Scenes/Piece.tscn")
var held_piece # the currently held/touched piece

var white_king # the white king piece
var white_king_check := false # is the white king in check?
var white_king_check_pieces = [] # pieces checking white king
var black_king # the black king piece
var black_king_check := false # is the black king in check?
var black_king_check_pieces = [] # pieces checking black king
onready var board := [
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null],
	[null,null,null,null,null,null,null,null]
]


func _ready():
	gui_banner.visible = true
	meta_banner.visible = true

	norm_scale = chessboard.rect_size/chessboard.texture.get_size()
	board_side = (chessboard.rect_size * chessboard.rect_scale).x
	box = Vector2(board_side, board_side)/tile_count # box size
	box_side = box.x
	chessboard.rect_position = Vector2(stepify(chessboard.rect_position.x, box_side), stepify(chessboard.rect_position.y, box_side))
	board_pos = chessboard.rect_position
	hoverbox.visible = false
	hoverbox.rect_scale = norm_scale
	chessboard_label.rect_position = board_pos
	# bg.rect_position = board_pos
	bg.rect_scale = norm_scale
	turn_label.get_child(0).rect_scale.x = 0
	player_colour = true
	reset_board(player_colour)

	for p in white_pieces:
		if player_colour:
			p.turn = true
		else:
			p.turn = false
	
	for p in black_pieces:
		if !player_colour:
			p.turn = true
		else:
			p.turn = false

	# read_fen_string("7k/3N2qp/b5r1/2p1Q1N1/Pp4PK/7P/1P3p2/pppppppp")
	# read_fen_string("7k/3N2qp/b5r1/2p1Q1N1/Pp4PK/7P/1P3p2/6r1")

	
func _process(_delta):
	if can_start:
		mouse_click_delay_rate = int(clamp(mouse_click_delay_rate, -100, mouse_click_delay))
		mouse_click_delay_rate -= 1
		# update()
		mouse_pos = get_viewport().get_mouse_position()
		tiled_mouse_pos = Vector2(stepify(mouse_pos.x - box.x/2, box.x), stepify(mouse_pos.y - box.y/2, box.x)) * chessboard.rect_scale
		# hoverbox.rect_position = tiled_mouse_pos
		if Input.get_action_strength("left_click") != 0 and mouse_click_delay_rate <= 0:
			mouse_click_delay_rate = mouse_click_delay
			picked_up = !picked_up # toggle pickup
			if picked_up:
				# prev_picked_pos = Vector2(clamp(tiled_mouse_pos.x/128, 0, board_side), clamp(tiled_mouse_pos.y/128, 0, board_side))
				prev_picked_pos = global_pos_to_board(tiled_mouse_pos)
				if (prev_picked_pos.x < tile_count and prev_picked_pos.y < tile_count):
					var piece = board[prev_picked_pos.y][prev_picked_pos.x]
					print(piece)
					if is_instance_valid(piece) and piece.turn:
						held_piece = piece
						piece.update_pattern(board)
						print(piece.pname)
						# piece.rect_position = tiled_mouse_pos
						hoverbox.rect_position = tiled_mouse_pos
						hoverbox.visible = true
					else:
						picked_up = false # only count piece pickup when an actual piece is selected
				else:
					picked_up = false # only count piece pickup when an actual piece is selected8
			else:
				# var cpp = Vector2(clamp(tiled_mouse_pos.x/128, 0, board_side), clamp(tiled_mouse_pos.y/128, 0, board_side))
				var cpp = global_pos_to_board(tiled_mouse_pos) # current position piece
				if (cpp.x < tile_count and cpp.y < tile_count):
					if cpp != prev_picked_pos:
						if move_piece(prev_picked_pos, cpp):
							switch_player()
							# print(len(moves))
							if len(moves) % 2 == 1:
								turns += 1
								moves_label.append_bbcode("%s%s. " % ["\n" if turns != 1 else "", turns])
							if board[cpp.y][cpp.x].pname[0].to_lower() == board[cpp.y][cpp.x].pname[0]:
								# black piece
								moves_label.append_bbcode("[color=black]%s[/color] " % moves[-1])
							else:
								# white piece
								moves_label.append_bbcode("[color=white]%s[/color] " % moves[-1])
							held_piece = null
							hoverbox.rect_position = tiled_mouse_pos
							hoverbox.visible = true
							for s in ds:
								s.queue_free()
							ds.clear()
						else:
							picked_up = true
					else:
						hoverbox.rect_position = tiled_mouse_pos
						hoverbox.visible = false
						held_piece = null
						for s in ds:
							s.queue_free()
						ds.clear()
				else:
					picked_up = true

		if picked_up and held_piece:
			held_piece.rect_position = tiled_mouse_pos
			# print(held_piece.pattern)
			if !ds:
				# for tp in held_piece.true_pattern:
				for p in held_piece.pattern:
					# for p in tp:
						var s = TextureRect.new()
						s.texture = load("res://Assets/blank.png")
						s.rect_position = board_to_global_pos(p)
						s.rect_scale = norm_scale
						s.modulate = Color(.5, .5, 1, .4)
						add_child(s)
						ds.append(s)


		# (DEBUG) check info about piece at location
		if Input.get_action_strength("right_click") != 0 and mouse_click_delay_rate <= 0:
			label.text = ""
			mouse_click_delay_rate = mouse_click_delay
			if point_on_board(mouse_pos):
				var ppp = board[global_pos_to_board(tiled_mouse_pos).y][global_pos_to_board(tiled_mouse_pos).x]
				if !ppp:
					label.text = "Null\nPosition: " + str(Vector2(global_pos_to_board(tiled_mouse_pos).x, global_pos_to_board(tiled_mouse_pos).y))
				else:
					label.text = ppp.pname + "\nPosition: " + str(ppp.position) + "\nDirection: " + str("up" if ppp.dir else "down") + "\nPattern: "
					for pppp in ppp.pattern:
						label.text += "\n\t" + str(pppp)
					label.text += "\nTrue Pattern: "
					for pppp in ppp.true_pattern:
						label.text += "\n\t" + str(pppp)
					
		if player_colour:
			turn_label.bbcode_text = "[color=white]White[/color]"
		else:
			turn_label.bbcode_text = "[color=black]Black[/color]"
			# turn_label.normal_font.size = 100
			# turn_label.default_color = Color(1, 1, 1)
			# turn_label.normal_font.outline_color = Color(0, 0, 0)
			
		# turn_label.text = ("White" if player_colour else "Black") + " to move" + "\n" + format_fen_string(write_fen_string()) + "\n" + str(point_on_board(mouse_pos))
		# turn_label.text = ("White" if player_colour else "Black") + " to move" + "\n" + write_fen_string() + "\n" + str(mouse_pos)


		if check_check() == 0:
			check_label.text = "White Wins!"
			can_start = false
			print("white wins!")
			win_banner.visible = true
			win_banner_text.bbcode_text = "[center][color=purple]WHITE WINS![/color][/center]"
			restart_button.disabled = false
			quit_button.disabled = false
		elif check_check() == 1:
			check_label.text = "Black Wins!"
			can_start = false
			print("black wins!")
			win_banner.visible = true
			win_banner_text.bbcode_text = "[center][color=purple]BLACK WINS![/color][/center]"
			restart_button.disabled = false
			quit_button.disabled = false

			
		if player_colour:
			if white_king_check:
				for p in white_king_check_pieces:
					if !cds:
						var s = TextureRect.new()
						s.texture = load("res://Assets/blank.png")
						s.rect_position = board_to_global_pos(p.position)
						s.rect_scale = norm_scale
						s.modulate = Color(.5, 1, .5, .4)
						add_child(s)
						cds.append(s)
						check_label.text = "White in check!"
			else:
				for s in cds:
					s.queue_free()
				cds.clear()
				check_label.text = ""
		else:
			if black_king_check:
				for p in black_king_check_pieces:
					if !cds:
						var s = TextureRect.new()
						s.texture = load("res://Assets/blank.png")
						s.rect_position = board_to_global_pos(p.position)
						s.rect_scale = norm_scale
						s.modulate = Color(.5, 1, .5, .4)
						add_child(s)
						cds.append(s)
						check_label.text = "Black in check!"
			else:
				for s in cds:
					s.queue_free()
				cds.clear()
				check_label.text = ""
			
			
		if !black_king_check and !white_king_check:
			for s in cds:
				s.queue_free()
			cds.clear()
			check_label.text = ""

	if Input.get_action_strength("reset") != 0 and mouse_click_delay_rate <= 0:
		mouse_click_delay_rate = mouse_click_delay
		# can_start = false
		reset_board(true)
		# can_start = true
		print("stuff")


func _notification(what):
	if what == MainLoop.NOTIFICATION_CRASH:
		print("whoops lol")
		# pass # the game crashed

		
func _draw():
	# var r := Rect2()
	# r.position = tiled_mouse_pos
	# r.size = box
	# # print(r.position)
	# draw_rect(r, Color8(255, 255, 255), false, 2)
	# draw_circle(mouse_pos, 4, Color8(255, 255, 255))
	pass

	
# reset board depending on player colour
func reset_board(c: bool):
	if c:
		# read_fen_string("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w") # standard
		read_fen_string("rnbqk1nr/ppp2ppp/8/1B1pp/1b1PP/8/PPP2PPP/RNBQK1NR b") # black and white in check
		# read_fen_string("7p/8/2QQQ/2QkQ/2QQQ/8/8/8 b") # idk
	else:
		read_fen_string("RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr b")


# remove all pieces from the board 
func clear_board():
	for r in range(tile_count):
		for c in range(tile_count):
			if is_instance_valid(board[r][c]):
				board[r][c].queue_free()
				board[r][c] = null
	if is_instance_valid(white_king):
		white_king.queue_free()
		white_king = null
	if is_instance_valid(black_king):
		black_king.queue_free()
		black_king = null
	for p in white_pieces:
		if is_instance_valid(p):
			p.queue_free()
			p = null
	white_pieces.clear()
	for p in black_pieces:
		if is_instance_valid(p):
			p.queue_free()
			p = null
	black_pieces.clear()
	for s in cds:
		s.queue_free()
	cds.clear()
	for s in ds:
		s.queue_free()
	ds.clear()
	if is_instance_valid(held_piece):
		held_piece.queue_free()
	held_piece = null
	picked_up = false
	moves.clear()
	moves_label.bbcode_text = ""
	turns = 0


# read in fen string and update chessboard
func read_fen_string(s: String):
	clear_board()

	var rank = 0
	var file = 0
	var strs = s.split(" ")

	for c in strs[0]:
		if c == "/":
			rank += 1
			file = 0
		else:
			if c.is_valid_integer():
				file += c.to_int()
			else:
				var piece = piece_instance.instance()
				piece_layer.add_child(piece)
				piece.pname = c
				piece.set_piece(c)
				piece.position = Vector2(file, rank)
				piece.rect_scale = norm_scale

				if c == c.to_lower():
					black_pieces.append(piece)
					if c == 'k':
						black_king = piece
				else: 
					white_pieces.append(piece)
					if c == 'K':
						white_king = piece
				# piece.dir = player_colour
				board[rank][file] = piece
				file += 1
	
	if strs[1]:
		player_colour = true 
		for p in white_pieces:
			p.turn = true
		for p in black_pieces:
			p.turn = false
	else:
		player_colour = false
		for p in black_pieces:
			p.turn = true
		for p in white_pieces:
			p.turn = false
	draw_board()


# create a fen string from the board
func write_fen_string() -> String:
	var tfen = ""
	for rank in range(tile_count):
		var spaces = 0
		for file in range(tile_count):
			var square = board[rank][file]
			if square:
				tfen += square.pname if spaces == 0 else str(spaces) + square.pname
				spaces = 0
			else:
				spaces += 1
		if spaces == tile_count:
			tfen += str(tile_count)
		tfen += "/"
	return tfen + " " + ("w" if player_colour else "b")
			

# get a fen string in a line-separate string
func format_fen_string(f: String):
	var tfens = f.split(" ")[0].split("/")
	var ret = ""
	for s in tfens:
		ret += s + "\n"
	return ret


# draw chessboard on screen		
func draw_board():
	for r in range(tile_count):
		for c in range(tile_count):
			if is_instance_valid(board[r][c]):
				var piece = board[r][c]
				piece.rect_position = board_to_global_pos(piece.position)


# check if the point is on the chessboard
func point_on_board(pos: Vector2) -> Vector2:
	var rr = Rect2(board_pos, chessboard.rect_size*chessboard.rect_scale)
	return rr.has_point(pos)

# translate + clamp a global position to a position on the chessboard
func global_pos_to_board(pos: Vector2) -> Vector2:
	# return Vector2(clamp(pos.x/box_side, 0, board_side), clamp(pos.y/box_side, 0, board_side))
	if !point_on_board(pos): 
		return Vector2.INF
	
	var tpos = pos - board_pos
	return Vector2(tpos.x/box_side, tpos.y/box_side)

# translate a chessboard position to a global position 
func board_to_global_pos(pos: Vector2) -> Vector2:
	return board_pos + Vector2(pos.x*box_side, pos.y*box_side)

# move a piece on a square to another square, using board coordinates and return whether or not the piece actually moved
func move_piece(from: Vector2, to: Vector2):
	var taken_piece = null
	var move_string = ""
	if is_instance_valid(board[to.y][to.x]):
		if board[to.y][to.x].colour == board[from.y][from.x].colour:
			picked_up = true
			held_piece = board[from.y][from.x]
			return false
		else:
			taken_piece = board[to.y][to.x]

	if board[from.y][from.x].can_move(to):
		board[to.y][to.x] = board[from.y][from.x]
		board[to.y][to.x].move(to)
		board[from.y][from.x] = null
		move_string += board[to.y][to.x].pname if board[to.y][to.x].pname.to_lower() != "p" else ""
	
		if taken_piece:
			white_pieces.erase(taken_piece)
			black_pieces.erase(taken_piece)
			taken_piece.queue_free()
			move_string += "x"
		var pppp = ['h', 'g', 'f', 'e', 'd', 'c', 'b', 'a']
		moves.append(move_string + pppp[to.x] + str(to.y))

		return true
	return false


# switch player turn
func switch_player():
	player_colour = !player_colour
	for p in white_pieces:
		if is_instance_valid(p):
			p.update_pattern(board)
			if player_colour:
				p.turn = true
			else:
				p.turn = false
	
	for p in black_pieces:
		if is_instance_valid(p):
			p.update_pattern(board)
			if !player_colour:
				p.turn = true
			else:
				p.turn = false
	
	# reverse order of fen string
	var tfen = fen.split(" ")
	fen = ""
	for p in range(len(tfen) - 1, 0, -1):
		fen += tfen[p]
	# read_fen_string(fen)


# check for checkmate of either colour.
# 0 if black is in checkmate
# 1 if white is in checkmate
# -1 if neither are in checkmate
func check_check() -> int:
	# ! this is kinda feverish and bad. 
	# ! should probably be in move_piece or something, and check checks should be global
	# ! ultimately, check every opposing piece pattern for the king, and return checkmate if it's in there
	# ! should update constantly, not depending on active colour (turn)

	# ! also, needs to block only attacking section, and prevents defending pieces from entering other potential sections
	# ! this currently does not work, and i can't be bothered to fix it
	
	var wkc := false # is the white king in check?
	var wkc_p = [] # black pieces checking white king
	var wkc_pp = [] # black piece patterns checking white king
	var bkc := false # is the black king in check?
	var bkc_p = [] # white pieces checking black king
	var bkc_pp = [] # white piece patterns checking black king


	
	# ------------------------------------------------ WHITE IN CHECK ------------------------------------------------
	for bp in black_pieces:
		# for each opposite piece
		if is_instance_valid(white_king) and is_instance_valid(bp) and white_king.position in bp.pattern:
			# if the piece can move to the moving king
			# the moving king is in check
			wkc = true
			wkc_p.append(bp)
			for wp in white_pieces:
				# for each moving piece (white)
				if wp == white_king:
					# if the moving king piece (white)
					for p in white_king.pattern:
						# for each king position
						for btp in bp.true_pattern:
							# for each section of the pieces unimpeded projecture
							if p in btp:
								# add pattern to checking patterns
								wkc_pp.append_array(btp)
								# if the king position is blocked off by the offending piece
								white_king.pattern.erase(p)
								# don't let the king move there
								for pds in ds:
									# remove the pattern shower
									if global_pos_to_board(pds.rect_position) == p:
										pds.queue_free()
										ds.erase(pds)
							else:
								continue
				else:
					# if moving regular piece (white)
					for wppa in wp.true_pattern:
						# for each section of the pieces unimpeded projecture
						for wpp in wppa:
							# for each piece position
							if wpp != bp.position and not wpp in bp.pattern and not wpp in wkc_pp:
								# if the position is not in the offending piece's pattern (i.e., if it can't block the offending piece's movement), and
								# if the position is not the offending piece itself (i.e., the piece is unable to be taken)
								wp.pattern.erase(wpp)
								# don't let the piece move there (i.e., only move a piece if it stops checkmate)
								for pds in ds:
									# remove the pattern shower
									if global_pos_to_board(pds.rect_position) == wpp:
										pds.queue_free()
										ds.erase(pds)
		
	white_king_check = wkc
	white_king_check_pieces = wkc_p
	
	# ------------------------------------------------ BLACK IN CHECK ------------------------------------------------
	for wp in white_pieces:
		# for each opposite piece
		if is_instance_valid(black_king) and is_instance_valid(wp) and black_king.position in wp.pattern:
			# if the piece can move to the moving king
			# the moving king is in check
			bkc = true
			bkc_p.append(wp)
			for bp in black_pieces:
				# for each moving piece
				if bp == black_king:
					# if the king piece
					for p in black_king.pattern:
						# for each king position
						if p in wp.pattern:
							# if the position is blocked off by the offending piece
							black_king.pattern.erase(p)
							# don't let the king move there
							for pds in ds:
								# remove the pattern shower
								if global_pos_to_board(pds.rect_position) == p:
									pds.queue_free()
									ds.erase(pds)
				else:
					# if regular piece
					for bpp in bp.pattern:
						# for each piece position
						if bpp != wp.position and not bpp in wp.pattern:
							# if the position is not in the offending piece's pattern (i.e., if it can't block the offending piece's movement)
							bp.pattern.erase(bpp)
							# don't let the piece move there (i.e., only move a piece if it stops checkmate)
							for pds in ds:
								# remove the pattern shower
								if global_pos_to_board(pds.rect_position) == bpp:
									pds.queue_free()
									ds.erase(pds)

		
	black_king_check = bkc
	black_king_check_pieces = bkc_p

	
	if !player_colour and !black_king.pattern and bkc:
		return 0
	elif player_colour and !white_king.pattern and wkc:
		return 1
	else:
		return -1

	# for wp in white_pieces:
	# 	if black_king.position in wp.pattern:
	# 		bkc = true
	# 		black_king_check = true
	# 		bkc_p.append(wp)
	
	# else:
	# 	check_label.text = "nothing"
	# 	for s in cds:
	# 		s.queue_free()
	# 	cds = []


func _on_RestartButton_pressed():
	reset_board(true)
	win_banner.visible = false
	restart_button.disabled = true
	quit_button.disabled = true
	can_start = true
	# check_check()


func _on_QuitButton_pressed():
	get_tree().quit()
