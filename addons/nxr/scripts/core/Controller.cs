using Godot;
using Godot.Collections;
using NXRInteractable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace NXR;

[GlobalClass]
public partial class Controller : XRController3D
{


	[Export]
	private float _linearVelocityStrength = 20f; 
	private float _angulerVelocityStrength = 20f; 
	private List<String> _buttonStates = new List<String>();
	private Array<Vector3> _localVels = new Array<Vector3>(); 
	private Array<Vector3> _globalVels = new Array<Vector3>(); 
	private Array<Vector3> _angularVels = new Array<Vector3>(); 
	private Vector3 _globalVelocity; 

	private List<Transform3D> velocityXforms = new(); 

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
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


	public Transform3D GetTransformVelocity()
    {
        // Get the current transform
        Transform3D currentTransform = GlobalTransform;


		velocityXforms.Add(currentTransform);

		if (velocityXforms.Count < 10) { 
			return currentTransform; 
		}

		if (velocityXforms.Count > 10) { 
			velocityXforms.RemoveAt(0); 
		}

        // Calculate the time interval since the last frame
        int ticks = Engine.PhysicsTicksPerSecond; 
	
        // Calculate the linear velocity (change in position)
        Vector3 linearVelocity = (velocityXforms.Last().Origin - velocityXforms.First().Origin);

        // Calculate the angular velocity (change in rotation)
        Quaternion currentRotation = velocityXforms.Last().Basis.GetRotationQuaternion();
        Quaternion previousRotation = velocityXforms.First().Basis.GetRotationQuaternion();
        Quaternion rotationChange = currentRotation * previousRotation.Inverse();
        Vector3 angularVelocity = rotationChange.GetEuler();

        // Create a new transform to represent the velocity
        Transform3D velocityTransform = new Transform3D();
        velocityTransform.Origin = linearVelocity;
        velocityTransform.Basis = Basis.FromEuler(angularVelocity);

        return velocityTransform;
    }

	public Vector3 GetLocalVelocity() { 
		Node3D parent = (Node3D)GetParent(); 
		return  parent.ToLocal(GetTransformVelocity().Origin); 
	}
	public Vector3 GetGlobalVelocity() { 
		return GetTransformVelocity().Origin * _linearVelocityStrength; 
	}
	public Vector3 GetAngularVelocity() { 
		return GetTransformVelocity().Basis.GetEuler() * _angulerVelocityStrength; 
	}

	public bool LocalVelMatches(Vector3 dir, float threshold) { 
		Node3D parent = (Node3D)GetParent(); 
		float dot = GetGlobalVelocity().Normalized().Dot(dir.Normalized()); 
		return dot > threshold; 
	}
}

