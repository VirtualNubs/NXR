using Godot;
using NXRInteractable;

namespace NXR;

[GlobalClass]
public partial class HandPose : RemoteTransform3D
{


	#region Exported
	[Export] private Interactable _interactable;
	[Export] private string _pose = "";
	[Export] private string _blendPose = ""; 
	[Export] private PoseType _poseType = PoseType.Pose;
	[Export] private GrabPose _grabPose = GrabPose.Primary;



	[ExportGroup("Pause Settings")]
	[Export] private string _pausePose;
	[Export] private bool _keepRemote = true;
	#endregion


	#region Private 
	private NodePath _lastPath;
	private Vector3 _initScale;
	private Transform3D _initXform; 
	private Transform3D _poseXform; 
	#endregion



	public override void _Ready()
	{
		GlobalScale(Vector3.One); 
		_initScale = Scale;
		_initXform = Transform; 

		if (_interactable == null && Util.NodeIs(GetParent(), typeof(Interactable)))
		{
			_interactable = (Interactable)GetParent();
		}

		if (_interactable == null) return;

		_interactable.OnGrabbed += OnGrab;
		_interactable.OnDropped += OnDrop;
	}

	public override void _Process(double delta)
	{
		GlobalScale(Vector3.One); 
 
	}


	private void OnGrab(Interactable interactable, Interactor interactor)
	{
		if (GetHand(interactor) == null) return;

		Hand hand = GetHand(interactor);

		if (_grabPose == GrabPose.Primary)
		{
			if (interactor == _interactable.PrimaryInteractor)
			{
				Pose(hand);
			}
		}

		if (_grabPose == GrabPose.Secondary)
		{
			if (interactor == _interactable.SecondaryInteractor)
			{
				Pose(hand);
			}
		}
	}

	private void OnDrop(Interactable interactable, Interactor interactor)
	{

		if (GetHand(interactor) == null) return;

		Hand hand = GetHand(interactor);

		if (hand.GetPath() == RemotePath)
		{
			RemotePath = "";
			hand.ResetHand();
		}

		if (hand.GetPath() == _lastPath)
		{
			_lastPath = "";
			hand.ResetHand();
		}
	}


	private void Pose(Hand hand)
	{

		_poseXform = _initXform; 

		if (hand.Scale.X < 0 || hand.Scale.X < 0)
		{
			_poseXform = _poseXform.Scaled(new Vector3(-1, 1, 1)); 
		}
		else
		{
			_poseXform = _poseXform.Scaled(_initScale); 
		}

		Transform = _poseXform; 
		GlobalTransform = GlobalTransform.Scaled(Vector3.One); 

		// set pose based on selected pose type 
		if (_poseType == PoseType.Pose)
		{
			hand.SetHandPose(_pose, _blendPose);
		}
	 

		PoseTween(hand); 
		// set RT paths
		_lastPath = hand.GetPath();
		RemotePath = hand.GetPath();
	}


	private void Pause()
	{
		if (_lastPath == null) return;

		if (!_keepRemote) RemotePath = null;

		if (IsInstanceValid(GetNode(_lastPath)))
		{
			Hand hand = (Hand)GetNode(_lastPath);
			hand.ResetHand(false);
			hand.SetHandPose(_pausePose, _blendPose);
		}
	}


	private Hand GetHand(Interactor interactor)
	{

		Hand hand = (Hand)Util.GetNodeFromParentOrOwnerType(this, typeof(Hand));


		if (hand == null)
		{
			foreach (Node child in interactor.GetChildren())
			{
				if (Util.NodeIs(child, typeof(Hand)))
				{
					hand = (Hand)child;
				}
			}
		}

		return hand;
	}


	private void PoseTween(Hand hand) { 

		GlobalTransform = hand.GlobalTransform; 

		Tween tween = GetTree().CreateTween(); 
		Transform3D newXform = _initXform.Scaled(Scale); 

		tween.TweenProperty(this, "transform", _poseXform, 0.1f); 
	}

	private void Reset()
	{   
		Hand hand = (Hand)GetNode(RemotePath);
		_lastPath = "";
		RemotePath = "";
		hand.ResetHand();
	}
}
