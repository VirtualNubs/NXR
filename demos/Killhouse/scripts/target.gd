extends RigidBody3D

@export var one_shot = false 


func hit(col, at):
	if one_shot: 
		$CollisionShape3D.disabled = true 
		
	$AnimationPlayer.play("hit")

func Reset(): 
	$CollisionShape3D.disabled = false 
	$AnimationPlayer.play("RESET")
