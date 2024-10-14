using Godot;

namespace NXRFirearm;

[GlobalClass]
public partial class FirearmBulletVisualizer : Node3D
{
	[Export] private FirearmMag _mag; 

	public override void _Process(double delta)
	{
		if (_mag == null) return; 
		
		for (int i = 0; i < GetChildren().Count; i++) { 
			if(i >= _mag.CurrentAmmo) { 
				Node3D child = (Node3D)GetChild(i); 
				child.Visible = false; 
			} else { 
				Node3D child = (Node3D)GetChild(i); 
				child.Visible = true; 
			}
		}
	}
}
