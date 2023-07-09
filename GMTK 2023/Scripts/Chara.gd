extends KinematicBody2D

onready var stage = get_parent() # the stage the character is in
onready var nav := $NavigationAgent2D
onready var ray := $RayCast2D
onready var coll := $CollisionShape2D
onready var spr := $Sprite

const FLOOR_NORMAL = Vector2.UP # the vector normal to the (horizontal) ground
# export var stomp_impulse = 1000.0
export var speed: = Vector2(800.0, 1000.0)
export var gravity := 4000.0
var velocity: = Vector2.ZERO
var path := PoolVector2Array()
var target : Vector2
var can_move := false
var about_to_fall := false
var tmp_frm := 0
var tmp_frm_pos := Vector2.ZERO

signal reached_goal

func _ready():
	tmp_frm_pos = position
	pass

func _process(_delta: float) -> void:
	tmp_frm += 1
	update()


func _physics_process(_delta: float) -> void:
	# if the raycast's collision point is below the chara position, or it isn't colliding with anything, the chara is about to fall
	about_to_fall = !ray.is_colliding() or (ray.get_collision_point().y != round(position.y + Vector2(coll.shape.radius, coll.shape.radius).y))
	# about_to_fall = !ray.is_colliding() or (ray.get_collision_point().y != round(position.y + coll.shape.extents.y))
	
	var direction = get_dir()
	velocity = calc_move_vel(velocity, direction, speed, false)
	nav.set_velocity(velocity)
	velocity = move_and_slide(velocity, FLOOR_NORMAL)
		
	if path.size():
		if position.distance_to(nav.get_next_location()) < .5:
			path.remove(0)
			if path.size():
				nav.set_target_location(path[0])
	elif path.size() < 2:
		# print(check.get_center())
		# check.position = Vector2(position.x - stage.gap/2, position.y - stage.gap/2)
		# check.size = Vector2(stage.gap, stage.gap)
		# if stage.roundToN(position, stage.gap) == stage.end_goal:
		if on_point(stage.end_goal):
			# print(stage.roundToN(position, stage.gap))
			emit_signal("reached_goal")
		
	can_move = false


# get the direction the character is moving in as a Vector2
func get_dir() -> Vector2:
	# return Vector2(
	# 	Input.get_action_strength("move_right") - Input.get_action_strength("move_left"),
	# 	-1.0 if Input.is_action_just_pressed("jump") and is_on_floor() else 1.0
	# )
	# print(tdir)
	# return Vector2(tdir.x, -1.0 if tdir.y < position.y-stage.gap*2 and is_on_floor() else 1.0)
	# print(ray.get)
	# if about_to_fall: print("%s, %s" % [about_to_fall, is_on_floor()])
	
	var tdir = position.direction_to(nav.get_next_location())
	var tdist = position.distance_to(nav.get_next_location())
	if tdir.x > 0:
		ray.position.x = stage.gap 
		spr.flip_h = true
	else:
		ray.position.x = -stage.gap
		spr.flip_h = false
	var aboves := 0
	for i in range(nav.get_nav_path_index(), nav.get_nav_path().size()):
		# if nav.get_nav_path_index() < nav.get_nav_path().size() - 2:
		# 	next_next_point = nav.get_nav_path()[nav.get_nav_path_index() + 1]
		if stepify(nav.get_nav_path()[i].x, stage.gap) == stepify(position.x, stage.gap) and nav.get_nav_path()[i].y < position.y:
			aboves += 1
			# next_next_point = nav.get_nav_path()[nav.get_nav_path_index() + 1]
		else:
			aboves += 1
			break
			# print(aboves)
				
	if tmp_frm >= 300:
		if stepify(position.x, stage.gap) == stepify(tmp_frm_pos.x, stage.gap):
			aboves += 5
			tdist += stage.gap
		tmp_frm = 0
		tmp_frm_pos = position

	var fdir = Vector2(tdir.x, 0)
	if ((tdir.y < -.5 or (about_to_fall and tdir.y < -0.1)) and is_on_floor()):
		fdir.y = -1.0
		# if next_next_point.x == nav.get_next_location().x:
		# 	speed.y = tdist * 40
		# else:
		speed.y = tdist * 20 * (aboves)
	else:
		fdir.y = 1.0
	
	return fdir
	# return Vector2(tdir.x, -1.0 if about_to_fall and is_on_floor() else 1.0)

# calculate move velocity as a Vector2
# takes in linear velocity, direction, speed, and jump interrupt boolean
func calc_move_vel(
		vel: Vector2,
		dir: Vector2,
		spd: Vector2,
		is_jump_interrupted: bool
	) -> Vector2:
	var nvel = vel # new velocity
	nvel.x = spd.x * dir.x
	nvel.y += gravity * get_physics_process_delta_time()
	if dir.y < 0:
		nvel.y = spd.y * dir.y # jump
	if is_jump_interrupted:
		nvel.y = 0.0
	return nvel

# is the chara on a certain point?
func on_point(v: Vector2) -> bool:
	var check := Rect2(position - Vector2(stage.gap/2.0, stage.gap/2.0), Vector2(stage.gap, stage.gap))
	return check.abs().has_point(v)

func _draw():
	# var check := Rect2(stage.roundToN(global_position - Vector2(stage.gap/2.0, stage.gap/2.0), stage.gap), Vector2(stage.gap, stage.gap))
	# print(check.position)
	# print(global_position)
	# draw_rect(check, Color.white)
	pass

# return navigation agent map (RID)
func get_agent_rid() -> RID:
	return nav.get_navigation_map()

func _on_Stage_chara_can_move(mp):
	can_move = true
	# nav.set_target_location(targ)
	path = mp
	if path.size(): path.remove(0)
	if path.size():
		nav.set_target_location(path[0])
	# print("can move")


func _on_NavigationAgent2D_target_reached():
	print("got to target")


func _on_NavigationAgent2D_velocity_computed(safe_velocity:Vector2):
	# var direction = get_dir()
	# var tv = calc_move_vel(velocity, direction, speed, false)
	# nav.set_velocity(tv)
	# velocity.y
	# velocity = move_and_slide(safe_velocity, FLOOR_NORMAL)
	pass
