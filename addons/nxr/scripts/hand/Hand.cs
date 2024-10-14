using System;
using System.Linq;
using Godot;
using Godot.NativeInterop;
using NXRInteractable;

namespace NXR;

[GlobalClass]
public partial class Hand : RigidBody3D
{

	#region Exported 
	[Export] public Interactor Interactor;
	[Export] private AnimationTree _animTree;

	[ExportGroup("Tool Setting")]
	[Export] private bool _mirrorX = false;
	[Export] private bool _mirrorY = false;
	[Export] private bool _mirrorZ = false;
	[Export] private bool _updateMirror = false;
	[Export] Skeleton3D _handSkeleton;
	#endregion


	#region  Private 
	protected Transform3D _initTransform;
	protected Vector3 _initRotation;
	protected Vector3 _initOffset;
	protected AnimationTree _currentAnimTree;
	protected string _lastBlendName;
	protected string _poseSpaceName = "PoseSpace";
	private Tween _resetTween; 
	#endregion


	public override void _Ready()
	{
		Transform = Transform.Orthonormalized();
		_initTransform = Transform;
		_initRotation = Rotation;
		Freeze = true;

		if (IsInstanceValid(Interactor))
		{
			_initOffset = Interactor.GlobalPosition - GlobalPosition;
		}

		if (IsInstanceValid(_animTree))
		{
			_currentAnimTree = _animTree;
		}

		if (IsInstanceValid(Interactor))
		{
			Interactor.Controller.InputFloatChanged += InputFloat;
			Interactor.Controller.InputVector2Changed += InputVec2;
			Interactor.Controller.ButtonPressed += ButtonPresed;
			Interactor.Controller.ButtonReleased += ButtonReleased;
		}
	}


	public override void _Process(double delta)
	{

		if (Engine.IsEditorHint()) return;

		if (
			IsInstanceValid(Interactor) &&
			Interactor.GrabbedInteractable != null &&
			!IsInstanceValid(Interactor.GrabbedInteractable
			))
		{
			//ResetHand();
		}
	}


	public void SetHandPose(String pose, String blend, float startY = 0)
	{
		AnimationNodeAnimation poseAnim = (AnimationNodeAnimation)GetPoseBlendSace().GetBlendPointNode(2);
		AnimationNodeAnimation blendAnim = (AnimationNodeAnimation)GetPoseBlendSace().GetBlendPointNode(0);

		_resetTween?.Stop(); 
		_animTree.Set("parameters/PoseTree/PoseSpace/blend_position", Vector2.Zero);


		Vector2 startPos = GetPoseBlendSace().GetBlendPointPosition(2);
		GetPoseBlendSace().SetBlendPointPosition(2, new Vector2(0, startY));

		String[] blendStripped = blend.Split(":");

		_lastBlendName = blendStripped[0];


		if (blendStripped.Last() != "")
		{
			blendAnim.Animation = blendStripped.Last();
		}
		else
		{
			blendAnim.Animation = pose;
		}

		poseAnim.Animation = pose;


		if (GetPoseTree().GetNode(_poseSpaceName) != null && blendStripped[0] != _poseSpaceName)
		{
			GetPoseTree().RenameNode(_poseSpaceName, blendStripped[0]);
			_poseSpaceName = blendStripped[0];
		}

		GetPlayback().Travel("PoseTree");
	}



	public void ResetHand(bool resetTransform = true)
	{
		if (resetTransform)
		{
			ResetTween();
		}

		GetPlayback().Travel("IdleTree");
	}


	private void InputFloat(String inputName, double value)
	{
		if (!IsInstanceValid(_currentAnimTree))
		{
			return;
		}

		Vector2 input = new(0, (float)value);
		_animTree.Set(string.Format("parameters/IdleTree/{0}/blend_amount", inputName), (float)value);
		_animTree.Set(string.Format("parameters/IdleTree/{0}/blend_position", inputName), (float)value);
		_animTree.Set(string.Format("parameters/PoseTree/{0}/blend_position", inputName), input);
	}


	private void InputVec2(String inputName, Vector2 value)
	{
		_animTree.Set(string.Format("parameters/PoseTree/{0}/blend_position", inputName), value);
	}


	private void ButtonPresed(string button)
	{
		string name = button + "_pressed";
		_animTree.Set(string.Format("parameters/IdleTree/{0}/blend_amount", name), 1);
		_animTree.Set(string.Format("parameters/IdleTree/{0}/blend_position", name), 1);
		_animTree.Set(string.Format("parameters/PoseTree/{0}/blend_position", name), 1);
	}


	private void ButtonReleased(string button)
	{
		string name = button + "_pressed";
		_animTree.Set(string.Format("parameters/IdleTree/{0}/blend_amount", name), 0);
		_animTree.Set(string.Format("parameters/IdleTree/{0}/blend_position", name), 0);
		_animTree.Set(string.Format("parameters/PoseTree/{0}/blend_position", name), 0);
	}


	private AnimationNodeBlendTree GetPoseTree()
	{
		AnimationNodeStateMachine stateMachine = (AnimationNodeStateMachine)_animTree.TreeRoot;
		AnimationNodeBlendTree poseTree = (AnimationNodeBlendTree)stateMachine.GetNode("PoseTree");
		return poseTree;
	}

	private AnimationNodeBlendTree GetIdleTree()
	{
		AnimationNodeStateMachine stateMachine = (AnimationNodeStateMachine)_animTree.TreeRoot;
		AnimationNodeBlendTree poseTree = (AnimationNodeBlendTree)stateMachine.GetNode("IdleTree");
		return poseTree;
	}

	private AnimationNodeStateMachine GetBaseStateMachine()
	{
		return (AnimationNodeStateMachine)_animTree.TreeRoot;
	}

	private AnimationNodeStateMachinePlayback GetPlayback()
	{
		return (AnimationNodeStateMachinePlayback)_animTree.Get("parameters/playback");
	}


	private AnimationNodeBlendSpace2D GetPoseBlendSace()
	{

		if (GetPoseTree().HasNode(_poseSpaceName))
		{

			return (AnimationNodeBlendSpace2D)GetPoseTree().GetNode(_poseSpaceName);
		}

		return null;
	}

	private void ResetTween()
	{
		_resetTween = GetTree().CreateTween();
		_resetTween.SetProcessMode(Tween.TweenProcessMode.Physics); 
		if (_resetTween != null) { 
			_resetTween.TweenProperty(this, "transform", _initTransform, 0.2f);
		}
	}
}
