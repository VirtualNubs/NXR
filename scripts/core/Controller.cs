using Godot;

namespace NXR;

[GlobalClass]
public partial class Controller : XRController3D
{
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

}
