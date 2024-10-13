using System;
using Godot;
using NXR;
using NXRFirearm;


[Tool]
[GlobalClass]
public partial class FirearmHammer : FirearmClampedXform
{

	[Export] private bool _singleAction = false; 

	private Firearm _firearm; 
	private float _triggerValue = 0f; 
	private bool _hammerReset = false; 
	private bool _lockedBack = false; 
	private float _threshold = 0.9f; 


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Util.NodeIs(GetParent(), typeof(Firearm))) { 
			_firearm = (Firearm)GetParent(); 
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RunTool(); 

		if (_firearm == null) return; 


		// if hammer reset move hamer based off trigger value 
		if(_hammerReset) {  
			float moveValue = _singleAction ? GetJoyValue() : GetJoyValue() + GetTriggerValue(); 
			StartToEnd(moveValue); 
		}

		if (AtEnd() && !_lockedBack) {
			_hammerReset = false;  
			_lockedBack = true; 
		}
		
		// when hammer is back stop moving and snap to start 
		if (GetTriggerValue() >= _threshold && _lockedBack) { 
			if (!_singleAction) _firearm.Fire(); 
			_lockedBack = false; 
			GoToStart(); 
		}
		
		// reset hammer if user trigger is released 
		if (GetTriggerValue() < 0.1f && AtStart()) {

			

			_hammerReset = true; 
		}

		if (!_lockedBack) { 
			_firearm.BlockFire = true; 
		} else{ 
			_firearm.BlockFire = false; 
		}
	}

	private float GetJoyValue() { 
		float joyY = 0f; 

		if (_firearm.GetPrimaryInteractor() != null) { 
			joyY = _firearm.GetPrimaryInteractor().Controller.GetVector2("primary").Y; 
		}
		joyY = Mathf.Clamp(joyY, -1f, 0.05f); 
		joyY = Math.Abs(joyY);

		if (joyY >= _threshold) joyY = 1; 

		return joyY; 
	}

	private float GetTriggerValue() { 
		return _firearm.GetTriggerValue(); 
	}
}
