using Godot;
using Godot.Collections;
using NXR;
using NXRFirearm;
using System;

[GlobalClass]
public partial class FirearmFiremodeSelector : Node
{

	[Export] private Firearm _firearm;
	[Export] private string _action = "primary_click";
	[Export] public Array<FireMode> ModeOrder { get; set; } = new Array<FireMode>();


	private int _currentMode { get; set; } = 0;



	public override void _Ready()
	{
		_firearm ??= FirearmUtil.GetFirearmFromParentOrOwner(this); 
		_firearm.FireMode = ModeOrder[0];
	}


	public override void _Process(double delta)
	{

		if (_firearm == null) return;
		if (_firearm.GetPrimaryInteractor() == null) return;

		if (_firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_action))
		{

			if (_currentMode < ModeOrder.Count - 1)
			{
				_currentMode += 1;
			}
			else
			{
				_currentMode = 0;
			}
			_firearm.FireMode = ModeOrder[_currentMode];
		}
	}
}
