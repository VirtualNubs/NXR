using Godot;
using NXR;
using NXRInteractable;


[GlobalClass]
public partial class InteractableGrabPoint : Interactable
{

    [Export] private Interactable _interactable = null;
    [Export] private GrabPointType _grabType = GrabPointType.Primary;


    private Vector3 _offset = new();


    public override void _Ready()
    {
        base._Ready();

        _interactable ??= (Interactable)Util.GetNodeFromParentOrOwnerType(this, typeof(Interactable));


        if (_interactable != null)
        {
            if (_grabType == GrabPointType.Primary)
            {
                _interactable.PrimaryGrabPoint = this;
            }
            else
            {
                _interactable.SecondaryGrabPoint = this;
            }

            _interactable.OnGrabbed += InteractableGrabbed;
            _interactable.OnDropped += InteractableDropped;
        }

        OnGrabbed += Grab;
    }


    private void Grab(Interactable interactable, Interactor interactor)
    {
        if (_interactable == null) return;

        FullDrop();

        if (_grabType == GrabPointType.Primary)
        {
            if (_interactable.GetPrimaryInteractor() == null)
            {
                _interactable.PrimaryGrabPoint = this;
                _interactable.Grab(interactor);
            }
        }

        if (_grabType == GrabPointType.Secondary)
        {
            if (_interactable.GetSecondaryInteractor() == null)
            {
                _interactable.SecondaryGrabPoint = this;
                _interactable.SecondaryGrab(interactor);
            }
        }

        if (_grabType == GrabPointType.Generic)
        {
            if (_interactable.GetPrimaryInteractor() == null)
            {
                _interactable.PrimaryGrabPoint = this;
                Scale = Vector3.One;
            }
            else
            {
                _interactable.SecondaryGrabPoint = this;
                Scale = Vector3.One * 3.0f;
            }
            _interactable.Grab(interactor);
        }
    }

    void InteractableGrabbed(Interactable interactable, Interactor interactor)
    {
        if (_grabType != GrabPointType.Generic) return; 

        if (interactable.PrimaryGrabPoint != this) { 
            interactable.SecondaryGrabPoint = this; 
            Scale = Vector3.One * 3.0f;
        }
    }

    void InteractableDropped(Interactable interactable, Interactor interactor)
    {
        if (_grabType != GrabPointType.Generic) return; 
        
        if (interactor == interactable.GetPrimaryInteractor()) {

            if (this == interactable.SecondaryGrabPoint) { 
                interactable.PrimaryGrabPoint = this; 
                interactable.SecondaryGrabPoint = interactable; 
            } 
        }
    }
}
