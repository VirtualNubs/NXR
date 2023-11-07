extends Node


@export var door: Node3D = null
var cleared: bool = false; 

var targets = []
var hit_count = 0

signal section_cleared

# Called when the node enters the scene tree for the first time.
func _ready():
	if get_parent().has_signal("completed"): 
		get_parent().connect("completed", completed)
	
	for child in get_children():
		if child.has_method("hit"): 
			targets.append(child)
			child.connect("target_hit", target_hit)
	
# Called every frame. 'delta' is the elapsed time since the previous frame


func target_hit(): 
	hit_count += 1
	
	if hit_count >= targets.size(): 
		emit_signal("section_cleared")
		cleared = true 
		door.get_node("AnimationPlayer").play("open")

func completed(): 
	for target in targets: 
		target.reset()
		door.get_node("AnimationPlayer").play("RESET")
	hit_count = 0
	door.get_node("AnimationPlayer").play("close")
	
func is_section(): 
	return true
