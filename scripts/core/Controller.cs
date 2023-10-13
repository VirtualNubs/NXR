using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace NXR;

[GlobalClass]
public partial class Controller : XRController3D
{

	private List<String> _buttonStates = new List<String>();

	private Array<Vector3> _globalVels = new Array<Vector3>(); 
	private Vector3 _globalVelocity; 

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
		_globalVelocity = GetXformVelocityAverage(_globalVels, GlobalTransform, delta); 
    }

    public void Pulse(double freq, double amp, double time)
	{
		// haptic pulse without delay
		XRServer.GetInterface(1).TriggerHapticPulse(
			"haptic",
			Tracker,
			freq,
			amp,
			time, 
			0.0
        ); 
	}

	public bool ButtonOneShot(string button)
	{
		if (IsButtonPressed(button) && !_buttonStates.Contains(button))
		{
			_buttonStates.Add(button);
			return true; 
		}

		if (!IsButtonPressed(button) && _buttonStates.Contains(button))
		{
		 _buttonStates.Remove(button); 
		}
		return false; 
	}

	public Vector3 GetXformVelocityAverage(Array<Vector3> vels, Transform3D xform, double delta) { 
		Vector3 o = xform.Origin; 
		vels.Add(o); 

		int vMult = 3; 

		if (vels.Count() > vMult) { 
			vels.RemoveAt(0); 
		}

		if (vels.Count() < vMult) { 
			return Vector3.Zero;  
		}

		var vel = Vector3.Zero; 
		foreach ( Vector3 v in vels) { 
			vel += v; 
		}
		vel /= vels.Count(); 

		var dir = o - vel; 
		vel = dir / (float)delta; 

		return vel; 
	}
	
	public Vector3 GetGlobalVelocity() {
		return _globalVelocity; 
	}
}

