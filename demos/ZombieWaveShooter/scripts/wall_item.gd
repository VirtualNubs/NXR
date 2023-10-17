extends Node

@export var price: int = 0
@export var bought = false
var grab_spawner: InteractableGrabSpawn = null
var label_price: Label3D = null 

# Called when the node enters the scene tree for the first time.
func _ready():
	grab_spawner = get_parent()
	grab_spawner.connect("OnGrabbed", grabbed)
	
	label_price = get_parent().get_node("LabelPrice")
	label_price.text = str(price)
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if !bought: 
		grab_spawner.Disabled = true 
		
		if ZombieDemoManager.current_money >= price: 
			label_price.modulate = Color.GREEN
		else: 
			label_price.modulate = Color.RED
	else: 
		label_price.visible = false 
		grab_spawner.Disabled = false 

func grabbed(interactable, interactor): 
	if ZombieDemoManager.current_money >= price: 
		bought = true 
		grab_spawner.SpawnAndGrab(interactor)
