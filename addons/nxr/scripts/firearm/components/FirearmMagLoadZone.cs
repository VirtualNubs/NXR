using Godot;
using NXR;
using NXRFirearm;
using NXRInteractable;
using System;

[GlobalClass]
public partial class FirearmMagLoadZone : Area3D
{
	[Export] private String _ammoGroup;
	[Export] private FirearmMag _mag; 


	[Signal] public delegate void AmmoAddedEventHandler(); 


	public override void _Ready()
	{
		BodyEntered += Entered; 
	}

	
	private void Entered(Node3D body) { 
			
		if (body.IsInGroup(_ammoGroup)) { 
			
			if (_mag.CurrentAmmo >= _mag.MaxAmmo) return; 

			Interactable interactable = null; 

			if (Util.NodeIs(body, typeof(Interactable))) { 
				interactable = (Interactable)body; 
			}

			if (_mag != null && _mag.CurrentAmmo < _mag.MaxAmmo) { 
				interactable?.FullDrop(); 
				body.QueueFree(); 
				_mag.AddBullet(1); 
				EmitSignal("AmmoAdded"); 
			}
		}
	}
}
