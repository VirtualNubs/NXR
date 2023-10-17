extends Node3D

@export var cost: int = 0 
@export var knob: Node3D = null
@export var unlocked = false

# Called when the node enters the scene tree for the first time.
func _ready():
	knob.connect("OnGrabbed", _grabbed)

func _grabbed(interactable, interactor) : 
	if (ZombieDemoManager.current_money >= cost): 
		get_node("AnimationPlayer").play("open")
		
		if !unlocked: 
			unlocked = true 
			ZombieDemoManager.current_money -= cost
