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
        base._Ready(); 
        
        if (Util.NodeIs((Node3D)GetParent(), typeof(Interactable)))
        {
            _interactable = (Interactable)GetParent();
            
            if (_grabType == GrabType.Primary)  {
                _interactable.PrimaryGrabPoint = this; 
                CallDeferred("SetOffset");
            } 
            if (_grabType == GrabType.Secondary)  {
                _interactable.SecondaryGrabPoint = this; 
                CallDeferred("SetOffset");
            }
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
                SetOffset(); 
            }
        }

        if (_grabType == GrabType.Secondary) { 
            if (_interactable.GetSecondaryInteractor() == null) { 
                _interactable.SecondaryGrabPoint = this; 
                _interactable.SecondaryGrab(interactor); 
                SetOffset(); 
            }
        }
    }

    private void SetOffset() { 


        if (_grabType == GrabType.Primary) {
            Transform3D newXform = new Transform3D(); 
            newXform.Origin = GlobalTransform.Origin -_interactable.GlobalTransform.Origin; 
            _interactable.PrimaryGrabPointOffset = newXform; 
            _interactable.SecondaryGrabPointOffset = newXform.Orthonormalized(); 
        }

        if(_grabType == GrabType.Secondary) {
            Transform3D newXform = new Transform3D(); 
            newXform.Origin = GlobalTransform.Origin - _interactable.GlobalTransform.Origin; 
            newXform.Basis = GlobalTransform.Basis; 
            _interactable.SecondaryGrabPointOffset = newXform.Orthonormalized(); 
        }
    }
}
