extends Node3D

@export var stage: Node3D
# Called when the node enters the scene tree for the first time.
func _ready():
	
	if get_parent() is Stage: 
		stage = get_parent()
	if stage: 
		stage.connect("ProgressUpdated", progress_updated)
		stage.connect("Loaded", loaded)
		stage.connect("TransitionQueued", queued)
		stage.connect("Transitioned", transitioned)
		
		recenter()
		
func _process(delta):
	pass
	
func queued(): 
	$SubViewport/Control/ProgressBar.value = 0
	$XROrigin3D/XRCamera3D/CSGSphere3D/AnimationPlayer.play("queue")

func progress_updated(p): 
	$SubViewport/Control/ProgressBar.value = p

func loaded(): 
	pass

func transitioned(): 
	$SubViewport/Control/ProgressBar.value = 100
	$XROrigin3D/XRCamera3D/CSGSphere3D/AnimationPlayer.play("transition")

func recenter(): 
	await  get_tree().create_timer(1).timeout
	XRServer.center_on_hmd(XRServer.RESET_BUT_KEEP_TILT, true)
