using Godot;
using NXR;
using NXRFirearm;
using NXRInteractable;
using System;

[GlobalClass]
public partial class FirearmMagLoadZone : Area3D
{
	[Export]
	private String _ammoGroup;

	[Export]
	private FirearmMag _mag; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += Entered; 
	}

	
	private void Entered(Node3D body) { 

			
		if (body.IsInGroup(_ammoGroup)) { 

			if (Util.NodeIs(body, typeof(Interactable))) { 

				Interactable interactable = (Interactable)body; 

				if (!interactable.IsGrabbed()) return; 

				interactable.FullDrop(); 
			}

			if (_mag != null && _mag.CurrentAmmo < _mag.MaxAmmo) { 

				body.QueueFree(); 
				_mag.AddBullet(1); 
			}
		}
	}
}
