using Godot;
using NXRInteractable; 

namespace NXR; 

[GlobalClass]
public partial class HandPose : RemoteTransform3D
{
    [Export]
    private string _pose = "";

    [Export]
    private PoseType _poseType = PoseType.Pose;

    [Export]
    private GrabPose _grabPose = GrabPose.Primary; 

    [Export]
    private AnimationTree _customTree; 

    private Interactable _interactable;
    private NodePath _lastPath;
    private Vector3 _initScale; 
    public override void _Ready()
    {
        if (!GetParent().HasMethod("IsInteractable")) return; 

        _interactable = (Interactable)GetParent(); 
        _interactable.OnGrabbed += OnGrab;
        _interactable.OnDropped += OnDrop;

        _initScale = Scale;  
    }

    private void OnGrab(Interactable interactable, Interactor interactor)
    {
        if (interactor.GetNode("NXRHand") == null) return;

        Hand hand = (Hand)interactor.GetNode("NXRHand");

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


        if (!IsInstanceValid(interactor) || interactor.GetNode("NXRHand") == null)  return;

        Hand hand = (Hand)interactor.GetNode("NXRHand");
        

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
        
        if (hand.Scale.X < 0) { 
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
            GD.Print("set custom tree"); 
            hand.SetCurrentTree(_customTree);
        }

        // set RT vars
        _lastPath = hand.GetPath();
        RemotePath = hand.GetPath(); 

    }

    private void Reset()
    {
        Hand hand = (Hand)GetNode(RemotePath);
        _lastPath = "";
        RemotePath = "";
        hand.ResetHand(); 
    }

}
