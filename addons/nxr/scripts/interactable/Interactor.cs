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
	public Interactable GrabbedInteractable { get; set; }
	private Interactable _distanceInteractable;


	[Export]
	public Controller Controller { get; private set; }

	[Export]
	private float _smoothing = 0.5f;

	[Export]
	private bool _updateTransform = true;


	#region Distance Grabbing 
	[Export]
	private bool _distanceGrabEnabled = true;
	private float _maxDistanceGrab = 5;
	private bool _distanceGrabbing = false;
	private float _distanceGrabDelta = 0.0f;
	#endregion


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
		if (_updateTransform)
		{
			GlobalTransform = GlobalTransform.InterpolateWith(Controller.GlobalTransform, _smoothing * (float)delta);
		}

		DistanceGrab(delta); 

	}

	public override void _Process(double delta)
	{
		// toggle drop
		if (IsInstanceValid(GrabbedInteractable) && GrabbedInteractable.HoldMode == HoldMode.Toggle && Controller.ButtonOneShot(GrabbedInteractable.GrabAction))
		{
			Drop();
		}
	}

	private void Interact(String buttonName)
	{

		if (IsInstanceValid(GrabbedInteractable)) { return; }

		// loop through hovered interactables
		foreach (Interactable hovered in HoveredInteractables())
		{

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

				if (interactable.DistanceGrabEnabled && dist < interactable.MaxDistanceGrab)
				{
					_distanceInteractable = interactable;
					_distanceGrabbing = true;
					_distanceGrabDelta = 0.0f;
					return;
				}
			}
		}
	}

	private void DistanceGrab(double delta)
	{
		if (_distanceGrabbing && IsInstanceValid(_distanceInteractable))
		{

			Transform3D xform = _distanceInteractable.GlobalTransform;
			xform.Origin = xform.Origin.Slerp(GlobalTransform.Origin, _distanceGrabDelta);

			_distanceInteractable.GlobalTransform = xform;

			float dist = GlobalPosition.DistanceTo(_distanceInteractable.GlobalPosition);
			if (dist <= _distanceInteractable.MaxGrabDistance)
			{
				_distanceGrabbing = false;
				Grab(_distanceInteractable);
			}

			_distanceGrabDelta += (float)delta;

		}
	}

	private void InteractDrop(String buttonName)
	{
		if (!IsInstanceValid(GrabbedInteractable))
		{
			return;
		}

		if (buttonName == GrabbedInteractable.GrabAction && GrabbedInteractable.HoldMode == HoldMode.Hold)
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

	public async void TweenGrab(Interactable interactable)
	{

		if (IsInstanceValid(GrabbedInteractable) || !_distanceGrabbing) return;

		Tween tween = GetTree().CreateTween();
		tween.SetProcessMode(Tween.TweenProcessMode.Physics);
		tween.TweenProperty(interactable, "global_position", GlobalPosition, 1f);

		await ToSignal(tween, "finished");
		_distanceGrabbing = false;
		Grab(interactable);
	}

	public void Grab(Interactable interactable)
	{
		GrabbedInteractable = interactable;
		interactable.Grab(this);
	}

	public void Drop()
	{
		GrabbedInteractable.Drop(this);
		GrabbedInteractable = null;
	}
}
