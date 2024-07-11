using Godot;
using NXRInteractable;

namespace NXRFirearm; 


[GlobalClass]
public partial class FirearmBullet : Interactable
{
	[Export] public MeshInstance3D Tip;

	public bool Spent = false;

	public void UpdateTip(bool spent)
	{
		if (Tip == null) return;

		Tip.Visible = !spent;
	}
	
}
