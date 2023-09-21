using Godot;
using System;
using System.Collections.Generic;

namespace NXR;

[GlobalClass]
public partial class Controller : XRController3D
{


	private List<String> _buttonStates = new List<String>();

    public override void _Ready()
    {
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
}
