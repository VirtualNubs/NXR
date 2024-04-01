extends Control

@export var player_settings: PlayerSettings

# Called when the node enters the scene tree for the first time.
func _ready():
	$VBoxContainer/HBoxContainer/DominantCheckButton.button_pressed = player_settings.DominantHand


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_check_button_toggled(toggled_on):
	if toggled_on: 
		player_settings.DominantHand = 1
	else:
		player_settings.DominantHand = 0
	
	player_settings.UpdateSettings(player_settings)
		
