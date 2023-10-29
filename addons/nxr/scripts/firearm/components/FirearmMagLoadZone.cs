using Godot;
using NXR;
using NXRInteractable;
using System;

[GlobalClass]
public partial class FirearmMagLoadZone : Area3D
{
	[Export]
	private String _ammoGroup;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += Entered; 
	}

	
	private void Entered(Node3D body) { 
		GD.Print(body.Name); 


		if (body.IsInGroup(_ammoGroup)) { 
			if (Util.NodeIs(body, typeof(Interactable))) { 
				Interactable interactable = (Interactable)body; 
				interactable.FullDrop(); 
			}
			body.QueueFree(); 
		}
	}
}
