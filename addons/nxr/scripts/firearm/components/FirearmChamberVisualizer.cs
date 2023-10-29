using Godot;
using System;

namespace NXRFirearm;

[GlobalClass]
public partial class FirearmChamberVisualizer : Node3D
{
	[Export]
	private Firearm _firearm;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
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
