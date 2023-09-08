extends HSlider

export var audio_bus_name := "Master"
onready var _bus := AudioServer.get_bus_index(audio_bus_name)

func _ready() -> void:
	value = db2linear(AudioServer.get_bus_volume_db(_bus))

func _on_HSlider_value_changed(value: float):
	AudioServer.set_bus_volume_db(_bus, linear2db(value))
	get_parent().get_node("MainVolumeAmount").bbcode_text = "[center]%s" % (value * 100)

