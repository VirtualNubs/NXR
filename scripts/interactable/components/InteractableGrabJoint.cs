using Godot;
using NXR;
using System;

[GlobalClass]
public partial class InteractableGrabJoint : Generic6DofJoint3D
{
    [Export]
    private GrabJointType _grabJointType;
    private Interactable _interactable;

    public override void _Ready()
    {
        if (!GetParent().HasMethod("IsInteractable")) {
            return;
        }

        _interactable = (Interactable)GetParent(); 


       if(_grabJointType == GrabJointType.Primary)
        {
            _interactable.PrimaryGrabJoint = this; 
        } else
        {
            _interactable.SecondaryGrabJoint = this; 
        }
    }
}
