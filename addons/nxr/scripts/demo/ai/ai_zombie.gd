extends CharacterBody3D

@export var target: Node3D

const SPEED = 0.2
const JUMP_VELOCITY = 4.5

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")


func _ready():
	var anim: AnimationPlayer = $zombie/metarig/GeneralSkeleton/AnimationPlayer
	anim.seek(
		randf_range(0, anim.current_animation_length)
	)
	Vector3.inv
	$zombie/metarig/GeneralSkeleton/AnimationPlayer.speed_scale = SPEED * 10
func _physics_process(delta):
	
	# Add the gravity.
	if not is_on_floor():
		velocity.y -= gravity * delta
	
	if target: 
		var dist = global_position.distance_to(target.global_position)
		var dir = target.global_position - global_position
		
		var dot = global_transform.basis.x.dot(dir.normalized())
		
		velocity = dir * SPEED
		
		rotate_y(deg_to_rad(dot))
		move_and_slide()

func hit(node, at): 
	$zombie/metarig/GeneralSkeleton/AnimationPlayer.stop()
	$zombie/metarig/GeneralSkeleton.physical_bones_start_simulation()
	ZombieDemoManager.current_money += 10
