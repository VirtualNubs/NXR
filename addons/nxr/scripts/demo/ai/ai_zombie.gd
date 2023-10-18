extends CharacterBody3D

@export var target: Node3D

const SPEED = 0.1
const JUMP_VELOCITY = 4.5

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")


func _ready():
	$AnimationTree.advance(randf_range(0, 1))

func _physics_process(delta):
	
	# Add the gravity.
	if not is_on_floor():
		velocity.y -= gravity * delta
	
	if target: 
		var dist = global_position.distance_to(target.global_position)
		var dir = target.global_position - global_position
		if dist < 2: 
			$AnimationTree.set("parameters/Attack/blend_amount", 0.5)
			
		else: 
			$AnimationTree.set("parameters/Attack/blend_amount", 0)
		
		var dot = global_transform.basis.x.dot(dir.normalized())
		
		velocity = dir * SPEED
		
		rotate_y(deg_to_rad(dot))
		move_and_slide()

func hit(node, at): 
	queue_free()
