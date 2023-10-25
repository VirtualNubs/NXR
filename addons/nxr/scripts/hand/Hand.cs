using System;
using System.Text.RegularExpressions;
using Godot;

namespace NXR;


[GlobalClass]
public partial class Hand : RigidBody3D
{

	[Export]
	private string _idleAnimation = "";

	[Export]
	public Controller Controller;

	[Export]
	private float _blendTime = 0.1f;

	[Export]
	private AnimationPlayer _animPlayer;

	[Export]
	private AnimationTree _animTree;

	private Transform3D _initTransform;
	private AnimationTree _currentAnimTree;


	[ExportGroup("Physics Hand Settings")]
	[Export]
    bool _physicsHand = false; 

	[Export]
	float linearStrength = 20.0f; 
	float angularStrength = 20.0f; 

	[Export]
	private CollisionShape3D _collider; 

	private Vector3 lVelocity = Vector3.Zero; 
	private Vector3 aVelocity = Vector3.Zero; 

    [Export]
    public Node3D HandNode = null; 


	private Transform3D _controllerOffsetXform = new(); 

	public override void _Ready()
	{
		Transform = Transform.Orthonormalized();
		_initTransform = Transform;

		if (IsInstanceValid(_animTree))
		{
			_currentAnimTree = _animTree;
		}

		if (IsInstanceValid(Controller))
		{
			Controller.InputFloatChanged += InputFloat;
			Controller.InputVector2Changed += InputVec;
		}

        if (!_physicsHand) { 
            Freeze = true; 
        }

        if (HandNode == null) { 
            HandNode = this; 
        }


		_initTransform = HandNode.Transform;
	}

    public override void _PhysicsProcess(double delta)
    {
        if (_collider != null) { 
			_collider.GlobalPosition = HandNode.GlobalPosition;
		}

		if (!_physicsHand && Controller != null) { 
			GlobalTransform = Controller.GlobalTransform;  
		}
    }
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
        if (!_physicsHand) return; 

        if (Controller == null) return; 

		float snapDist = Controller.GlobalPosition.DistanceTo(GlobalPosition);
		float dist = Controller.GlobalPosition.DistanceTo(GlobalPosition) * 200;
		
		Vector3 dir = Controller.GlobalPosition - GlobalPosition;
		Quaternion currentRotation = GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion previousRotation = Controller.GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion rotationChange = currentRotation * previousRotation.Inverse();
		Vector3 angularVelocity = rotationChange.Inverse().GetEuler();

        if (snapDist > 0.5) { 
            GlobalPosition = Controller.GlobalTransform.Origin; 
        }

		lVelocity = lVelocity.Lerp(dir * linearStrength , 0.5f); 
		aVelocity = aVelocity.Lerp(angularVelocity * angularStrength, 0.5f); 

		float distStrength = (linearStrength / dist); 
		distStrength = Mathf.Clamp(distStrength, 0.001f, 1.0f); 

		state.LinearVelocity = state.LinearVelocity.Lerp(lVelocity, distStrength);
		state.AngularVelocity = state.AngularVelocity.Lerp(aVelocity, distStrength); 
	}

	public void SetHandPose(String pose)
	{
		_currentAnimTree.Active = false;

		if (!IsInstanceValid(_animPlayer))
		{
			GD.PushWarning("No animation player found!");
			return;
		}

		_animPlayer.Play(_idleAnimation);
		_animPlayer.Advance(0);
		_animPlayer.Play(pose, _blendTime);
	}

	public void SetCurrentTree(AnimationTree tree)
	{
		_currentAnimTree.Active = false;
		tree.Active = false;
		tree.AnimPlayer = "";

		_currentAnimTree = tree;
		_currentAnimTree.AnimPlayer = _animPlayer.GetPath();
		_currentAnimTree.Active = true;
		_currentAnimTree.Advance(0);
	}

	public void ResetHand(bool resetTransform = true)
	{

		if (_animPlayer == null)
		{
			GD.PushWarning("No AnimationPlayer Found!");
			return;
		}

		if (resetTransform)
		{
			HandNode.Transform = _initTransform;
		}

		if (_idleAnimation != "" && _animPlayer.HasAnimation(_idleAnimation))
		{
			_animPlayer.Play(_idleAnimation);
		}

		if (IsInstanceValid(_animTree))
		{
			SetCurrentTree(_animTree);
		}
	}

	private void InputFloat(String inputName, double value)
	{
		if (!IsInstanceValid(_currentAnimTree))
		{
			return;
		}

		_currentAnimTree.Set(string.Format("parameters/{0}/blend_amount", inputName), (float)value);
		_currentAnimTree.Set(string.Format("parameters/{0}/blend_position", inputName), (float)value);
	}

	private void InputVec(String inputName, Vector2 value)
	{

	}

	public bool IsHand() { 
		return true; 
	}

    public bool IsPhysicsHand() { 
        return _physicsHand;
    }


}
