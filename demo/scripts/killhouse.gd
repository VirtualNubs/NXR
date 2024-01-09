extends Node3D

@export var start_door: KillhouseDoor
@export var end_door: KillhouseDoor
var sections = []
var cleared = 0
var started = false; 

signal completed

# Called when the node enters the scene tree for the first time.
func _ready():
	for child in get_children(): 
		if child.has_method("is_section"): 
			sections.append(child)

func _on_start_area_body_entered(body):
	if body is Player and !started:
		started = true 

func _on_end_area_body_entered(body):
	if body is Player and started:
		emit_signal("completed")
		end_door.close()
		started = false
