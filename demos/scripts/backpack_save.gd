extends Node

var path = ""
# Called when the node enters the scene tree for the first time.
func _ready():
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
 
func save():
	await get_tree().create_timer(0.5).timeout
	
	var file = FileAccess.file_exists(get_parent().scene_file_path)
	var path = get_parent().scene_file_path
	var packed = PackedScene.new()
	packed.pack(get_parent())
	
	await get_tree().create_timer(0.5).timeout
	
	ResourceSaver.save(packed, "res://demos/scenes/backpack.tscn")

func _on_interactable_snap_zone_3_on_snap(interactable):
	save()


func _on_area_3d_on_item_added():
	save()

func _on_area_3d_on_item_removed():
	save()
