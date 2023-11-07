using Godot;
using NXRInteractable; 

namespace NXR; 

[GlobalClass]
public partial class HandPose : RemoteTransform3D
{
    [Export]
    private Interactable _interactable;

    [Export]
    private string _pose = "";

    [Export]
    private PoseType _poseType = PoseType.Pose;

    [Export]
    private GrabPose _grabPose = GrabPose.Primary; 

    [Export]
    private AnimationTree _customTree; 

    [ExportGroup("Pause Settings")]
    [Export]
    private string _pausePose; 

    [Export]
    private bool _keepRemote = true; 



    private NodePath _lastPath;
    private Vector3 _initScale; 

    
    public override void _Ready()
    {

        _initScale = Scale;  
        
        if (_interactable == null && Util.NodeIs(GetParent(), typeof(Interactable))) { 
            _interactable = (Interactable)GetParent(); 
        } 

        if (_interactable == null) return; 

        _interactable.OnGrabbed += OnGrab;
        _interactable.OnDropped += OnDrop;
        

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

        if (hand.HandNode.GetPath() == RemotePath)
        {
            RemotePath = "";
            hand.ResetHand(); 
        }

        if (hand.HandNode.GetPath() == _lastPath)
        {
            _lastPath = "";
            hand.ResetHand();
        }
    }

    private void Pose(Hand hand)
    {
        
        if (hand.HandNode.Scale.X < 0) { 
            Scale = new Vector3(-1, 1, 1); 
        } else { 
            Scale = _initScale; 
        }

        // set pose based on selected pose type 
        if (_poseType == PoseType.Pose)
        {
            hand.SetHandPose(_pose); 
        }
        if (_poseType == PoseType.Tree && _customTree != null)
        {
            hand.SetCurrentTree(_customTree);
        }

        
        // set RT paths
        _lastPath = hand.HandNode.GetPath();
        RemotePath = hand.HandNode.GetPath(); 
        
    }

    private void Pause() { 
        if (_lastPath == null) return; 

        if (!_keepRemote) RemotePath = null; 

        if (IsInstanceValid(GetNode(_lastPath))) { 
            Hand hand = (Hand)GetNode(_lastPath); 
            hand.ResetHand(false); 
            hand.SetHandPose(_pausePose); 
        }
    }

    private Hand GetHand(Interactor interactor) { 

        Hand hand = null;

        if(interactor.GetParent().HasMethod("IsHand")) { 
            hand = (Hand)interactor.GetParent();
        } else { 
            foreach (Node3D child in interactor.GetChildren()) { 
                if (child.HasMethod("IsHand")) { 
                    hand = (Hand)child; 
                } 
            }
        }
        
        return hand; 
    }

    private void Reset()
    {
        Hand hand = (Hand)GetNode(RemotePath);
        _lastPath = "";
        RemotePath = "";
        hand.ResetHand(); 
    }
}
