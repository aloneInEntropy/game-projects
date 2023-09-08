extends Node2D


onready var stage := get_parent()
onready var tg := stage.get_node("GUI/TutorialGuide")
onready var tgt := stage.get_node("GUI/TutorialGuide/GuideText")
onready var tgbg := stage.get_node("GUI/TutorialGuide/GTBG")
onready var ttimer := $TxtTimer

onready var frame := 0

var tout := false # has the timer timed out?
export var p_amt := 1.0 ## pause amount, in seconds

export var tt1_completed := false
var tt1 := [false, false, false]
export var tt2_completed := false
var tt2 := [false, false, false]
export var tt3_completed := false
var tt3 := [false, false]
export var tt4_completed := false
var tt4 := [false, false]
export var tt5_completed := false
var tt5 := [false, false]
export var tt6_completed := false
var tt6 := [false, false, false]
export var tt7_completed := false
var tt7 := [false, false, false]
export var tt8_completed := false
var tt8 := [false]

func _ready():
	ttimer.start(p_amt)
	tgt.bbcode_text = ""
	stage.can_move_cam = false

func _process(_delta):
	frame += 1
	
	if stage.can_pause:
		tg.visible = !stage.is_paused
	else:
		tg.visible = true

	if !tt1_completed: 
		anim_1()
	if tt1_completed and !tt2_completed: 
		anim_2()
	if tt2_completed and !tt3_completed:
		anim_3()
		stage.can_move_cam = true
	if get_viewport_transform().origin.abs().x > get_viewport_rect().size.x/3:
		tt3_completed = true
	if tt3_completed and !tt4_completed: 
		anim_4()
	if tt4_completed and !tt5_completed:
		stage.best_line.visible = true
		anim_5()
	if tt5_completed and !tt6_completed:
		anim_6()
	if tt6_completed and !tt7_completed:
		stage.extra_place_timer = 300
		stage.extra_cut_timer = 300
		anim_7()
	if tt7_completed and !tt8_completed:
		anim_8()


func center_bb(txt: String, n: RichTextLabel, newtxt: bool):
	if newtxt:
		n.bbcode_text = "[center]" + txt
	else:
		n.append_bbcode("[center]" + txt)


func _on_Stage_update_cuts(_val: int):
	if !tt1_completed and tt1[-1]:
		tt1_completed = true
		tgt.bbcode_text = ""
	
	if tt1_completed and !tt2_completed and tt2[-1]:
		print("now")
		tt2_completed = true
		tgt.bbcode_text = ""


func _on_Stage_update_places(_val: int):
	if tt3_completed and !tt4_completed and tt4[-1]:
		# print("now")
		tt4_completed = true
		tgt.bbcode_text = ""


func _on_Stage_update_targets(_val: int):
	if tt4_completed and !tt5_completed and tt5[-1]:
		# print("now")
		tt5_completed = true
		tgt.bbcode_text = ""
	
	if tt5_completed and !tt6_completed and tt6[-1]:
		# print("now")
		tt6_completed = true
		tgt.bbcode_text = ""

func restart_timer(val: float):
	ttimer.start(val)
	tout = false


func anim_1():
	tgbg.rect_position = Vector2(672, 443)
	tgbg.rect_size = Vector2(352, 165)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt1[0]:
		center_bb("This is Ai. She is the [b]FAULTY ROBOT[/b] you created. She wants to save all the princesses, and you must help her.", tgt, true)
		restart_timer(p_amt)
		tt1[0] = true
		
	if tout and !tt1[1]:
		center_bb("You have the ability to edit the level by either [b]CUTTING[/b] or [b]PLACING[/b] tiles.\n\n", tgt, false)
		restart_timer(p_amt)
		tt1[1] = true
		
	if tout and !tt1[2]:
		center_bb("Press [b]RIGHT CLICK to destroy a tile.[/b]", tgt, false)
		restart_timer(p_amt)
		stage.update_cuts(1)
		tt1[2] = true


func anim_2():
	tgbg.rect_position = Vector2(672, 420)
	tgbg.rect_size = Vector2(352, 180)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)
	

	if tout and !tt2[0]:
		center_bb("Altered tiles will be reverted after a little while.\n\n", tgt, true)
		restart_timer(p_amt)
		tt2[0] = true

	if tout and !tt2[1]:
		center_bb("Ai will jump over gaps and obstacles whenever necessary to reach her goal. This includes staircases such as this.\n\n", tgt, false)
		restart_timer(p_amt)
		tt2[1] = true
		
	if tout and !tt2[2]:
		center_bb("Press [b]RIGHT CLICK[/b] to destroy a tile and proceed.", tgt, false)
		restart_timer(p_amt)
		stage.update_cuts(1)
		tt2[2] = true


func anim_3():
	tgbg.rect_position = Vector2(672, 430)
	tgbg.rect_size = Vector2(352, 170)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt3[0]:
		center_bb("You can move the camera around by dragging your mouse while holding the [b]MIDDLE MOUSE BUTTON[/b] or [b]CTRL + LEFT MOUSE BUTTON[/b].\n\n", tgt, true)
		restart_timer(p_amt)
		tt3[0] = true
	
	if tout and !tt3[1]:
		center_bb("Alternatively, you can press the [b]SPACEBAR[/b] to automatically move the camera to Ai.", tgt, false)
		restart_timer(p_amt)
		tt3[1] = true
		

func anim_4():
	tgbg.rect_position = Vector2(0, 460)
	tgbg.rect_size = Vector2(352, 200)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt4[0]:
		center_bb("You can place tiles by pressing [b]LEFT MOUSE BUTTON[/b]. You cannot destroy these tiles.\n\n", tgt, true)
		restart_timer(p_amt)
		tt4[0] = true
		
	if tout and !tt4[1]:
		center_bb("Try to place a tile underneath Ai so she can jump over.", tgt, false)
		restart_timer(p_amt)
		stage.update_places(5)
		tt4[1] = true


func anim_5():
	tgbg.rect_position = Vector2(0, 420)
	tgbg.rect_size = Vector2(374, 180)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt5[0]:
		center_bb("The line onscreen is a [b]guideline[/b] leading directly to the nearest [b]target[/b]. You can disable it in the [b]settings[/b].\n\n", tgt, true)
		restart_timer(p_amt)
		tt5[0] = true
	
	if tout and !tt5[1]:
		center_bb("It turns red when where is no available path towards any target. When this happens, Ai will move towards the nearest target, even if she can't reach it.", tgt, false)
		restart_timer(p_amt)
		tt5[1] = true


func anim_6():
	tgbg.rect_position = Vector2(0, 270)
	tgbg.rect_size = Vector2(404, 330)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt6[0]:
		center_bb("When Ai reaches a target, you gain one cut and one place (usually). The target becomes a physical block you can cut and Ai can touch.\n\n", tgt, true)
		restart_timer(p_amt)
		tt6[0] = true
	
	if tout and !tt6[1]:
		center_bb("When Ai is in range of multiple targets, she may get confused on which one to get. You can [b]DOUBLE LEFT CLICK[/b] on a target to [b]highlight[/b] it. Ai will attempt to move towards that target singularly, even if she can't reach it, and she won't be able to collect any other targets.\n\n", tgt, false)
		restart_timer(p_amt)
		tt6[1] = true
	
	if tout and !tt6[2]:
		center_bb("You can disable the targeting by doubleclicking on the target again or highlighting another target, and Ai will recalibrate.", tgt, false)
		restart_timer(p_amt)
		tt6[2] = true


func anim_7():
	tgbg.rect_position = Vector2(0, 320)
	tgbg.rect_size = Vector2(404, 280)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt7[0]:
		center_bb("You can press [b]ESCAPE[/b] or [b]ENTER[/b] to open the [b]pause menu[/b], or press the icon in the top left corner.\n\n", tgt, true)
		restart_timer(p_amt)
		tt7[0] = true
	
	if tout and !tt7[1]:
		center_bb("Pressing [b]R[/b] will restart the current level from the beginning. This is accessible from the pause menu.\n\n", tgt, false)
		restart_timer(p_amt)
		tt7[1] = true
	
	if tout and !tt7[2]:
		center_bb("Pressing [b]TAB[/b] will open the minimap. Clicking the targets in the minimap will highlight them once the menu is closed. The guide line in the minimap can be toggled in the settings. This is also accessible from the pause menu.", tgt, false)
		restart_timer(p_amt)
		tt7[2] = true

func anim_8():
	tgbg.rect_position = Vector2(0, 570)
	tgbg.rect_size = Vector2(1024, 280)
	tgt.rect_position = tgbg.rect_position + GM.single_vector(8)
	tgt.rect_size = tgbg.rect_size - GM.single_vector(16)

	if tout and !tt8[0]:
		center_bb("Please enjoy the game! Help Ai achieve her goals, and maybe you can achieve your goals too!\n\n", tgt, true)
		restart_timer(p_amt)
		tt8[0] = true


func _on_TxtTimer_timeout():
	tout = true


func _on_Stage_game_over(win: bool):
	if win:
		if tt6_completed and !tt7_completed and tt7[-1]:
			tt7_completed = true
			tgt.bbcode_text = ""


