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

	#region Exported: 
	[Export] public Controller Controller { get; private set; }
	[Export] public float Smoothing { get; set; } = 0f;
	[Export] public bool UpdateTransform { get; set; } = true;
	#endregion


	#region Public: 
	public Interactable GrabbedInteractable { get; set; }
	public RigidBody3D InteractorBody = new RigidBody3D();
	#endregion


	#region Private: 
	protected Interactable _distanceInteractable;
	protected bool _distanceGrabbing = false;
	private float _distanceGrabDelta = 0.0f;
	private float _distancePointBias = 0.95f;
	#endregion


	[Signal] public delegate void GrabbedEventHandler(Interactable interactable);
	[Signal] public delegate void DroppedEventHandler(Interactable interactable);


	public override void _Ready()
	{
		Controller.ButtonPressed += Interact;
		Controller.ButtonReleased += InteractDrop;
		BodyExited += BodyExit;
		InteractorBody.FreezeMode = RigidBody3D.FreezeModeEnum.Static;
	}


	public override void _PhysicsProcess(double delta)
	{

		if (UpdateTransform)
		{
			GlobalTransform = GlobalTransform.InterpolateWith(Controller.GlobalTransform, Mathf.Clamp(1 - Smoothing, 0.001f, 1.0f));
		}
		
		if (_distanceInteractable != null) { 
			_distanceInteractable.LinearVelocity = Vector3.Zero; 
			_distanceInteractable.GlobalPosition = _distanceInteractable.GlobalPosition.Lerp(GlobalPosition, (float)delta * 10.0f); 
			_distanceInteractable.GlobalBasis = _distanceInteractable.GlobalBasis.Slerp(GlobalBasis, (float)delta * 10.0f); 
			if (_distanceInteractable.GlobalPosition.DistanceTo(GlobalPosition) < _distanceInteractable.GrabBreakDistance) { 
				Grab(_distanceInteractable); 
				_distanceInteractable = null; 
				
			}
		}
	}


	public override void _Process(double delta)
	{
		if (HoveredInteractables().Count > 0 && !HoveredInteractables()[0].IsGrabbed()) { 
			HoveredInteractables()[0].Hovered = true; 
		}
	}


	private void Interact(String buttonName)
	{

		if (IsInstanceValid(GrabbedInteractable)) {
			
			if (GrabbedInteractable.HoldMode == HoldMode.Toggle && GrabbedInteractable.DropAction == buttonName) { 
				Drop(); 
			}

			return; 
		}
		
		foreach (Interactable hovered in HoveredInteractables())
		{

			if (buttonName == hovered.GrabAction)
			{

				Interactable interactable = hovered;
				float dist = GlobalPosition.DistanceTo(interactable.GlobalPosition);

				if (dist <= interactable.GrabBreakDistance && !_distanceGrabbing)
				{
					Grab(interactable);
					return;
				}


				if (interactable.PrimaryInteractor == null && interactable.DistanceGrabEnabled && dist < interactable.DistanceGrabReach)
				{
					_distanceInteractable = interactable;
					
					
					return;
				}
			}
		}
	}

	
	private void InteractDrop(String buttonName)
	{

		if (GrabbedInteractable != null && buttonName == GrabbedInteractable.GrabAction && GrabbedInteractable.HoldMode == HoldMode.Hold)
		{
			Drop();
			return; 
		}
		
		if (_distanceInteractable != null && buttonName == _distanceInteractable.GrabAction)
		{
			_distanceInteractable.LinearVelocity = Vector3.Zero; 
			_distanceInteractable  = null; 
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
				Interactable interactable = (Interactable)node;
				Vector3 dir = interactable.GlobalPosition - GlobalPosition; 
				float dist = GlobalPosition.DistanceTo(interactable.GlobalPosition);
				float dot = -GlobalBasis.Z.Dot(dir.Normalized()); 
				bool closeGrab = dist <= interactable.GrabBreakDistance; 
				bool distanceGrab = interactable.DistanceGrabEnabled && dist <= interactable.DistanceGrabReach && dot >= _distancePointBias; 

				if (closeGrab || distanceGrab)
				{
					interactables.Add((Interactable)node);
				} else if (interactable != null) { 
					interactable.Hovered = false; 
				}
			}
		}

		// sort closest hovered interactable 
		interactables = interactables.OrderBy(x => {
			float distanceToGrab = 1f;

			if (x.GetPrimaryInteractor() == null)
			{
				distanceToGrab = x.PrimaryGrabPoint.GlobalPosition.DistanceTo(GlobalPosition);
			} else
			{
				distanceToGrab = x.SecondaryGrabPoint.GlobalPosition.DistanceTo(GlobalPosition);
			}

			return distanceToGrab / x.Priority;
		}).ToList();

		
		return interactables;
	}


	public void Grab(Interactable interactable)
	{
		GrabbedInteractable = interactable;
		interactable.Grab(this);
		_distanceGrabbing = false;

		EmitSignal("Grabbed", interactable);
	}


	public void Drop()
	{
		Interactable interactable = GrabbedInteractable; ;

		GrabbedInteractable.Drop(this);
		GrabbedInteractable = null;
		_distanceGrabDelta = 0.0f;

		EmitSignal("Dropped", interactable);
	}

	private void BodyExit(Node3D body)
	{

		if (Util.NodeIs(body, typeof(Interactable)))
		{
			Interactable interactable = (Interactable)body;

			interactable.Hovered = false;
		}
	}
	
	private void DistanceGrabFinished() { 
		Grab(_distanceInteractable); 
	}
}