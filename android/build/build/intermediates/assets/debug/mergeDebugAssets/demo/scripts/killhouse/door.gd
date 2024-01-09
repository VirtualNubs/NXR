extends Node3D

class_name KillhouseDoor

signal opened
signal closed

@onready var animation_player = $AnimationPlayer


func open(): 
	animation_player.play("open")
	emit_signal("opened")

func close(): 
	animation_player.play("close")
	emit_signal("closed")
