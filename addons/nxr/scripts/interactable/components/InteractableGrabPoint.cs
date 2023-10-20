using Godot;
using NXR;
using NXRInteractable; 


[GlobalClass]
public partial class InteractableGrabPoint : Interactable
{   
 
    [Export]
    private GrabType _grabType = GrabType.Primary; 

    private Interactable _interactable = null; 
    private Vector3 _offset = new(); 

    public override void _Ready()
    {
         if (Util.NodeIs((Node3D)GetParent(), typeof(Interactable)))
        {
            _interactable = (Interactable)GetParent();
        }
        
        OnGrabbed += Grab; 
    }

    private void Grab(Interactable interactable, Interactor interactor) { 

        if (_interactable == null) return; 

        FullDrop(); 
        
        if (_grabType == GrabType.Primary) { 
            if (_interactable.GetPrimaryInteractor() == null) { 
                _interactable.PrimaryGrabPoint = this; 
                _interactable.Grab(interactor); 
            }
        }

        if (_grabType == GrabType.Secondary) { 
            if (_interactable.GetSecondaryInteractor() == null) { 
                _interactable.SecondaryGrabPoint = this; 
                _interactable.SecondaryGrab(interactor); 
            }
        }
    }
}
