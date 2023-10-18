using Godot;
using System;
using System.Runtime.CompilerServices;
using NXRInteractable; 


[GlobalClass]
public partial class InteractableGrabPoint : Marker3D
{   
    [Export]
    private GrabType _grabType = GrabType.Primary;

    private Interactable _interactable = null; 
    private Vector3 _offset = new(); 

    public override void _Ready()
    {
        _interactable = (Interactable)GetParent(); 
        if (_grabType == GrabType.Primary) {
            _offset = _interactable.ToLocal(_interactable.GlobalPosition) - Position; 
            _interactable.PositionOffset += _offset; 
            _interactable.RotationOffset += RotationDegrees; 
            _interactable.PrimaryGrabPoint = this; 
        } else{ 
            _interactable.SecondaryGrabPoint = this;
        }
    }
}
