extends Control


onready var level_won_texture := preload("res://Resources/torch_anim.tres")

func _ready():
	print(GM.levels_completed)
	for i in range(len(get_tree().get_nodes_in_group("lvlb"))):
		if GM.levels_completed[i]:
			get_tree().get_nodes_in_group("lvlb")[i].texture_normal = level_won_texture
	pass

func _on_Level1_pressed():
	GM.play_level(1)


func _on_Level2_pressed():
	GM.play_level(2)


func _on_Level3_pressed():
	GM.play_level(3)


func _on_Level4_pressed():
	GM.play_level(4)


func _on_Level5_pressed():
	GM.play_level(5)


func _on_Level6_pressed():
	GM.play_level(6)


func _on_Level7_pressed():
	GM.play_level(7)


func _on_Level8_pressed():
	GM.play_level(8)


# func _on_Level9_pressed():
# 	print("Level9")


# func _on_Level10_pressed():
# 	print("Level10")


func _on_Tutorial_pressed():
	GM.play_level(0)
