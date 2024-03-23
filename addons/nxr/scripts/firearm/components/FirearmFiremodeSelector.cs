using Godot;
using Godot.Collections;
using NXR;
using NXRFirearm;


[GlobalClass]
public partial class FirearmFiremodeSelector : Node
{


	[Export]public int CurrentMode { get; set; } = 0;
	[Export]private string _action = "primary_click";
	[Export]public Array<FireMode> ModeOrder { get; set; } = new Array<FireMode>();
	
	private Firearm _firearm;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Util.NodeIs(GetParent(), typeof(Firearm))) _firearm = (Firearm)GetParent();
		if (Util.NodeIs(Owner, typeof(Firearm))) _firearm = (Firearm)Owner;

		_firearm.FireMode = ModeOrder[CurrentMode];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (_firearm == null) return;
		if (_firearm.GetPrimaryInteractor() == null) return;

		if (_firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_action))
		{

			if (CurrentMode < ModeOrder.Count - 1)
			{
				CurrentMode += 1;
			}
			else
			{
				CurrentMode = 0;
			}
			_firearm.FireMode = ModeOrder[CurrentMode];
		}
	}
}
