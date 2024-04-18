using Godot;
using NXRInteractable;

namespace NXRFirearm; 


[GlobalClass]
public partial class FirearmBullet : Interactable
{
	[Export] public MeshInstance3D tip;

	public bool Spent = false;

	public void updateTip(bool spent)
	{
		tip.Visible = !spent;
	}
	
}
