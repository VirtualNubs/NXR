using Godot;
using System;
using NXR;
using NXRInteractable;
using System.Linq;
using System.Collections.Generic;
public partial class InteractableFloatArea : Area3D
{
	private List<Interactable> _bodies = new(); 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += Entered;
		BodyExited += Exited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (Interactable body in _bodies)
		{
			GD.Print(body.Name); 
			body.Freeze = true; 
		}
	}

	public void Entered(Node3D body)
	{
		if (body == GetParent()) return; 
		if (Util.NodeIs(body, typeof(Interactable)))
		{
			Interactable interactable = (Interactable)body;

			if (!interactable.IsGrabbed()) return;

			interactable.Reparent(this); 
			_bodies.Add(interactable); 
		}
	}

	public void Exited(Node3D body) { 

		if (Util.NodeIs(body, typeof(Interactable)))
		{
			foreach (Interactable interactable in _bodies) {
				if (body == interactable) { 
					_bodies.Remove(interactable); 
					interactable.Reparent(interactable.InitParent); 
				}
			}
		}
	}
}
