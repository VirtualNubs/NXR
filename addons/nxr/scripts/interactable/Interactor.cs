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

		if (_grabbedInteractable != null) {
			Interactable interactable = _grabbedInteractable; 

			// if (this == interactable.PrimaryInteractor && GlobalPosition.DistanceTo(interactable.PrimaryGrabPoint.GlobalPosition) > interactable.MaxGrabDistance) {
			// 	Drop(); 
			// }

			// if (this == interactable.SecondaryInteractor && GlobalPosition.DistanceTo(interactable.SecondaryGrabPoint.GlobalPosition) > interactable.MaxGrabDistance) {
			// 	Drop(); 
			// }
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


		foreach (Interactable hovered in HoveredInteractables()) {

			if (buttonName == hovered.GrabAction)
			{
				Interactable interactable = hovered;
				float dist = GlobalPosition.DistanceTo(interactable.GlobalPosition);

				if (dist <= interactable.MaxGrabDistance)
				{
					Grab(interactable);
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


	public void Grab(Interactable interactable)
	{
		interactable.Grab(this);
	}

	public void Drop()
	{
		_grabbedInteractable.Drop(this);
		_grabbedInteractable = null;

	}
}
