using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using NXR; 

namespace NXRInteractable;

[GlobalClass]
public partial class Interactor : Area3D
{

	[Export]
	public Controller Controller;

	[Export]
	private float _smoothing = 0.5f; 
	public Interactable _grabbedInteractable;

	[Export]
	private bool _updateTransform = true; 


	[Export]
	private bool _distanceGrabEnabled = true; 
	private float _maxDistanceGrab = 5; 
	private bool _distanceGrabbing = false; 
	private float _distanceGrabDelta = 0.0f; 

	private Interactable _distanceInteractable; 

	public RigidBody3D PhysicsGrabBody = new(); 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddChild(PhysicsGrabBody);
		PhysicsGrabBody.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
		PhysicsGrabBody.Freeze = true;
		PhysicsGrabBody.Position = Vector3.Zero; 

		Controller.ButtonPressed += Interact;
		Controller.ButtonReleased += InteractDrop;
	}

	public override void _PhysicsProcess(double delta)
	{
		
		// follow controller transform 
		if (_updateTransform) { 
			GlobalTransform = GlobalTransform.InterpolateWith(Controller.GlobalTransform, _smoothing * (float)delta);
		}
		
		// distance grabbing
		if (_distanceGrabbing && _distanceInteractable != null) { 
			
			Transform3D xform = _distanceInteractable.GlobalTransform; 
			xform.Origin = xform.Origin.Slerp(GlobalTransform.Origin, _distanceGrabDelta); 

			_distanceInteractable.GlobalTransform = xform; 

			float dist = GlobalPosition.DistanceTo(_distanceInteractable.GlobalPosition);
			if (dist <= _distanceInteractable.MaxGrabDistance) { 
				_distanceGrabbing = false; 
				Grab(_distanceInteractable); 
			}

			_distanceGrabDelta += (float)delta; 

		}
	}

    public override void _Process(double delta)
    {
		// toggle drop
		if (IsInstanceValid(_grabbedInteractable) && _grabbedInteractable.HoldMode == HoldMode.Toggle && Controller.ButtonOneShot(_grabbedInteractable.GrabAction))
		{
			Drop();
		}
    }
	
    private void Interact(String buttonName)
	{
	
		if (_grabbedInteractable != null) { return; }

		// loop through hovered interactables
		foreach (Interactable hovered in HoveredInteractables()) {
			
			// check input  
			if (buttonName == hovered.GrabAction)
			{
				
				Interactable interactable = hovered;
				float dist = GlobalPosition.DistanceTo(interactable.GlobalPosition);

				if (dist <= interactable.MaxGrabDistance && !_distanceGrabbing)
				{
					Grab(interactable);
					return; 
				}

				if (interactable.DistanceGrabEnabled && dist < interactable.MaxDistanceGrab) { 
					_distanceInteractable = interactable; 
					_distanceGrabbing = true; 
					_distanceGrabDelta = 0.0f; 
					return;
				}
			}
		}
	}

	private void InteractDrop(String buttonName)
	{
		if (!IsInstanceValid(_grabbedInteractable))
		{
			return;
		}

		if (buttonName == _grabbedInteractable.GrabAction && _grabbedInteractable.HoldMode == HoldMode.Hold)
		{
			Drop();
		}
	}

	private List<Interactable> HoveredInteractables()
	{
		Array<Node3D> bodies = GetOverlappingBodies();
		List<Interactable> interactables = new List<Interactable>();

		foreach (Node3D node in bodies)
		{
			if (Util.NodeIs(node, typeof(Interactable)))
			{
				interactables.Add((Interactable)node);
			}
		}
		
		// sort closest hovered interactable 
		interactables = interactables.OrderBy(x => x.GlobalPosition.DistanceTo(GlobalPosition) / (int)x.Priority).ToList();

		return interactables;
	}

	public async void TweenGrab(Interactable interactable) { 

		if (_grabbedInteractable != null || !_distanceGrabbing) return; 

		Tween tween = GetTree().CreateTween();
		tween.SetProcessMode(Tween.TweenProcessMode.Physics); 
		tween.TweenProperty(interactable, "global_position", GlobalPosition, 1f);  
		
		await ToSignal(tween, "finished"); 
		_distanceGrabbing = false; 
		Grab(interactable); 
	}

	public void Grab(Interactable interactable)
	{
		_grabbedInteractable = interactable; 
		interactable.Grab(this);
	}

	public void Drop()
	{
		_grabbedInteractable.Drop(this);
		_grabbedInteractable = null;
	}
}
