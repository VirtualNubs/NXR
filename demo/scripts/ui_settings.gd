extends Button


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_pressed():
	if get_tree().root.has_node("Stage"): 
		var stage: Stage = get_tree().root.get_node("Stage")
		stage.QueueTransition("res://demo/StageTest.tscn")
