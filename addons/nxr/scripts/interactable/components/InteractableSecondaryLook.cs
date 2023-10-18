using Godot;
using NXR;
using System;

namespace NXRInteractable;


[GlobalClass]
public partial class InteractableSecondaryLook : Node
{
    [Export]
    private LookUpVector _lookUpVector = LookUpVector.PrimaryInteractor;

    [Export]
    private bool _invert = false;
    private Interactable _interactable;

    private bool _initUpdaterRotation = false; 

    public override void _Ready()
    {
        if (Util.NodeIs((Node3D)GetParent(), typeof(Interactable)))
        {
            _interactable = (Interactable)GetParent();
        }
        else
        {
            GD.PushWarning("No Interactable found!");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsInstanceValid(_interactable)) return; 

        if (_interactable.IsTwoHanded())
        {
            Look();
        }
    }

    public void Look()
    {
        Transform3D lookXform = _interactable.GlobalTransform;
        Transform3D secondaryXform = _interactable.SecondaryInteractor.GlobalTransform;
        Vector3 lookDir = secondaryXform.Origin - _interactable.PrimaryInteractor.GlobalTransform.Origin;
        Vector3 up = _interactable.GlobalTransform.Basis.Y  + GetUpVector();

        if (_invert) { lookDir = -lookDir; }

        _interactable._primaryGrabTransorm.Basis = _interactable.Basis; 

        lookXform.Basis = Basis.LookingAt(lookDir.Normalized(), up.Normalized());
        _interactable.GlobalTransform = lookXform;
    }

    public Vector3 GetUpVector()
    {
        switch (_lookUpVector)
        {
            case LookUpVector.PrimaryInteractor:
                return _interactable.PrimaryInteractor.GlobalTransform.Basis.Y;
            case LookUpVector.SecondaryInteractor:
                return _interactable.SecondaryInteractor.GlobalTransform.Basis.Y;
            case LookUpVector.Combined:
                return (_interactable.PrimaryInteractor.GlobalTransform.Basis.Y + _interactable.SecondaryInteractor.GlobalTransform.Basis.Y).Normalized();
        }

        return Vector3.Up;
    }
}
