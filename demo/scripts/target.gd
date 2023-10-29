extends RigidBody3D

@export var one_shot = false 
var is_hit = false

func hit(col, at):
	if is_hit: 
		return
	
	if one_shot: 
		$CollisionShape3D.disabled = true 
	else: 
		$ResetTimer.start()
	$AnimationPlayer.play("hit")
	
	is_hit = true
	
func Reset(): 
	$CollisionShape3D.disabled = false 
	$AnimationPlayer.play("RESET")
	is_hit = false 

func _on_reset_timer_timeout():
	if !one_shot: 
		Reset()
