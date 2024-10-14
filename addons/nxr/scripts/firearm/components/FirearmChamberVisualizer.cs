using Godot;
using System;

namespace NXRFirearm;

[GlobalClass]
public partial class FirearmChamberVisualizer : Node3D
{
	[Export] private Firearm _firearm;

	public override void _Process(double delta)
	{
		if(_firearm != null) { 
			if (_firearm.Chambered) { 
				Visible = true; 
			} else{ 
				Visible = false; 
			}
		}
	}
}
