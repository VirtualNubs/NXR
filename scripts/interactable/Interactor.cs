using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

namespace NXR;

[GlobalClass]
public partial class Interactor : Area3D
{

	[Export]
	public Controller Controller;

	private Interactable _grabbedInteractable;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Controller.ButtonPressed += Interact;
		Controller.ButtonReleased += InteractDrop;
	}


	public override void _PhysicsProcess(double delta)
	{
		// follow controller transform 
		GlobalTransform = Controller.GlobalTransform;
	}


	private void Interact(String buttonName)
	{
		// toggle drop
		if (IsInstanceValid(_grabbedInteractable) && _grabbedInteractable.HoldMode == HoldMode.Toggle && buttonName == _grabbedInteractable.GrabAction)
		{
			Drop();
			return;
		}


		if (HoveredInteractables().Count > 0 && buttonName == HoveredInteractables()[0].GrabAction)
		{
			Interactable interactable = HoveredInteractables()[0];
			float dist = GlobalPosition.DistanceTo(interactable.GlobalPosition);

			if (dist <= interactable.MaxGrabDistance)
			{
				Grab(interactable);
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
			if (node.HasMethod("IsInteractable"))
			{
				interactables.Add((Interactable)node);
			}
		}

		// sort closest hovered interactable 
		interactables = interactables.OrderBy(x => x.GlobalPosition.DistanceTo(GlobalPosition)).ToList();

		return interactables;

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
