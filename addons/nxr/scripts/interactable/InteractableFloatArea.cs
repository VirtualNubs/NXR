using Godot;
using System;
using NXR;
using NXRInteractable;
using System.Linq;
using System.Collections.Generic;
public partial class InteractableFloatArea : Area3D
{
	private List<Interactable> _bodies = new(); 

	[Signal]
	public delegate void OnItemAddedEventHandler(); 
	
	[Signal]
	public delegate void OnItemRemovedEventHandler(); 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AreaEntered += Entered;
		AreaExited += Exited;

		foreach (Node child in GetChildren()) { 
			if (Util.NodeIs(child, typeof(Interactable))) { 
				_bodies.Add((Interactable)child); 
				GD.Print(child.Name); 
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (Interactable body in _bodies)
		{
			body.Freeze = true; 
		}
	}

	public void Entered(Node3D area)
	{

		if (!Util.NodeIs(area, typeof(Interactor))) return; 


		Interactor interactor = (Interactor)area;

		if (interactor._grabbedInteractable == null) return;

		_bodies.Add(interactor._grabbedInteractable); 
		interactor._grabbedInteractable.Reparent(this);
		interactor._grabbedInteractable.Owner = GetParent();  
		EmitSignal("OnItemAdded"); 
	}

	public void Exited(Node3D area) { 

		if (!Util.NodeIs(area, typeof(Interactor))) return; 


		Interactor interactor = (Interactor)area;

		if (interactor._grabbedInteractable == null) return;

		interactor._grabbedInteractable.Reparent(GetTree().CurrentScene); 
		_bodies.Remove(interactor._grabbedInteractable); 
		EmitSignal("OnItemRemoved"); 
	}
}
