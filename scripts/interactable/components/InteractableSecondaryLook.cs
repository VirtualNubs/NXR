using Godot;
using System;

namespace NXR;

[GlobalClass]
public partial class InteractableSecondaryLook : Node
{
    [Export]
    private LookUpVector _lookUpVector = LookUpVector.PrimaryInteractor; 

    private Interactable _interactable;


    public override void _Ready()
    {
        if (GetParent().HasMethod("IsInteractable"))
        {
            _interactable = (Interactable)GetParent();
        }
        else
        {
            GD.PushWarning("No Interactable found!");
        }
    }


    public override void _Process(double delta)
    {
        if (!IsInstanceValid(_interactable) || !IsInstanceValid(_interactable.SecondaryInteractor)) return;

        Transform3D lookXform = _interactable.GlobalTransform;
        Transform3D secondaryXform = _interactable.SecondaryInteractor.GlobalTransform;
        Vector3 rotOffset = _interactable.RotationOffset * (Vector3.One * (Mathf.Pi / 180));
        Vector3 lookDir = secondaryXform.Origin - _interactable.PrimaryInteractor.GlobalTransform.Origin;
        Vector3 up = lookXform.Basis.Y + GetUpVector();

        lookXform.Basis = Basis.LookingAt(lookDir.Normalized(), up.Normalized()) * Basis.FromEuler(rotOffset); 
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
