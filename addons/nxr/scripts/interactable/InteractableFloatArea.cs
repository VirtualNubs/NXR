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

		if (interactor.GrabbedInteractable == null) return;

		Connect(interactor.GrabbedInteractable); 
		EmitSignal("OnItemAdded"); 
	}

	public void Exited(Node3D area) { 

		if (!Util.NodeIs(area, typeof(Interactor))) return; 


		Interactor interactor = (Interactor)area;

		if (interactor.GrabbedInteractable == null) return;

		Disconnect(interactor.GrabbedInteractable); 
		EmitSignal("OnItemRemoved"); 
	}
	
	private void OnDropped(Interactable interactable, Interactor interactor) { 
		Transform3D xform = interactable.GlobalTransform; 

		_bodies.Add(interactable); 
		interactable.Freeze = true;
		interactable.Reparent(this);
		interactable.Owner = GetParent(); 

		interactable.SetDeferred("global_transform", xform); 
	}

	private void OnGrabbed(Interactable interactable, Interactor interactor) { 

		_bodies.Remove(interactable); 
		interactable.Freeze = interactable.InitFreeze;
		interactable.Reparent(interactable.InitParent);
		interactable.Owner = interactable.InitParent;  

	}

	 private void Connect(Interactable interactable)
    {
        Action<Interactable, Interactor> dropAction = OnDropped;
        Action<Interactable, Interactor> grabAction = OnGrabbed;
        bool dropConnected = interactable.IsConnected("OnDropped", Callable.From(dropAction));
        bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

        if (!dropConnected)
        {
            interactable.Connect("OnDropped", Callable.From(dropAction));
        }
        if (!grabConnected)
        {
            interactable.Connect("OnGrabbed", Callable.From(grabAction));
        }
    }

    private void Disconnect(Interactable interactable)
    {
        if (interactable == null) return;

        Action<Interactable, Interactor> dropAction = OnDropped;
        Action<Interactable, Interactor> grabAction = OnGrabbed;
        bool dropConnected = interactable.IsConnected("OnDropped", Callable.From(dropAction));
        bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

        if (dropConnected)
        {
            interactable.Disconnect("OnDropped", Callable.From(dropAction));
        }
        if (grabConnected)
        {
            interactable.Disconnect("OnGrabbed", Callable.From(grabAction));
        }
    }
}
