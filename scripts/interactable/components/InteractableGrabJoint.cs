using Godot;
using NXR;
using System;

namespace NXRInteractable; 

[GlobalClass]
public partial class InteractableGrabJoint : Generic6DofJoint3D
{
    [Export]
    private Interactable _interactable;

    public override void _Ready()
    {
        
    }
}
