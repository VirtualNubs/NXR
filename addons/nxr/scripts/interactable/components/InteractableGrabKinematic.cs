using Godot;
using NXR;
using NXRInteractable;

[GlobalClass]
public partial class InteractableGrabKinematic : Node
{

    [Export]
    private bool _percise = false;


    [ExportGroup("TwoHandSettings")]
    [Export]
    private bool _twoHanded = false;
    [Export]
    private LookUpVector _lookUpVector = LookUpVector.PrimaryInteractor;
    [Export]
    private bool invert = false; 


    [ExportGroup("SmoothingSettings")]
    [Export(PropertyHint.Range, "0.0, 1")]
    public float PositionSmoothing = 1f;
    [Export(PropertyHint.Range, "0.0, 1")]
    public float RotationSmoothing = 1f;


    private Vector3 _perciseOffset = new();

    private bool _initFreezeState = false;

    public Interactable Interactable;
    private Transform3D _primaryXform = new(); 
    public override void _Ready()
    {
        if (Interactable == null && Util.NodeIs((Node3D)GetParent(), typeof(Interactable)))
        {
            Interactable = (Interactable)GetParent();
            Interactable.OnGrabbed += OnGrab;
            Interactable.OnDropped += OnDrop;
            Interactable.OnFullDropped += OnFullDrop;
            _initFreezeState = Interactable.Freeze;
        }
    }

    public override void _PhysicsProcess(double delta)
    {   
        if (Interactable.IsGrabbed()) { Interactable.LinearVelocity = Vector3.Zero; }

        if (IsInstanceValid(Interactable.PrimaryInteractor))
        {
            _primaryXform = Interactable.PrimaryInteractor.GlobalTransform;
            _primaryXform.Basis = _primaryXform.Basis.Slerp(_primaryXform.Basis, RotationSmoothing);
            _primaryXform.Origin = Interactable.GlobalTransform.Origin.Lerp(_primaryXform.Origin, PositionSmoothing);

            if (_percise) { _primaryXform = Interactable.GetPrimaryRelativeXform(); }
            
            if (!_twoHanded)
            {
                Interactable.GlobalTransform = _primaryXform * Interactable.GetOffsetXform();
            }
            else
            {
                if (Interactable.IsTwoHanded())
                {
                    Interactable.GlobalTransform = TwoHandXform() * Interactable.GetOffsetXform();
                }
                else
                {
                    Interactable.GlobalTransform = _primaryXform * Interactable.GetOffsetXform();
                }
            }
        }

        if (IsInstanceValid(Interactable.SecondaryInteractor) && !IsInstanceValid(Interactable.PrimaryInteractor)) { 
            Interactable.GlobalTransform = Interactable.GetSecondaryRelativeXform(); 
        }
    }

    public void OnGrab(Interactable interactable, Interactor interactor)
    {
        if (interactor == Interactable.PrimaryInteractor)
        {
            Interactable.Freeze = true;
            Interactable.LinearVelocity = Vector3.Zero;
        }
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        _perciseOffset = Vector3.Zero;
    }

    public void OnFullDrop() { 

        Interactable.Freeze = _initFreezeState;
    }

    public Transform3D TwoHandXform()
    {

        Transform3D lookXform = _primaryXform;
        Transform3D secondaryXform = Interactable.SecondaryInteractor.GlobalTransform;
        Vector3 up = Interactable.GlobalTransform.Basis.Y + GetUpVector();
        Vector3 lookDir = secondaryXform.Origin - Interactable.PrimaryInteractor.GlobalTransform.Origin;
        Vector3 offset = Interactable.SecondaryGrabPoint.GlobalPosition - Interactable.PrimaryGrabPoint.GlobalPosition; 
                

        Interactable._primaryGrabTransorm.Basis = Interactable.Basis;
        lookXform.Basis = Basis.LookingAt(lookDir.Normalized(), up.Normalized()).Orthonormalized();
        return lookXform;
    }

    public Vector3 GetUpVector()
    {
        switch (_lookUpVector)
        {
            case LookUpVector.PrimaryInteractor:
                return Interactable.PrimaryInteractor.GlobalTransform.Basis.Y;
            case LookUpVector.SecondaryInteractor:
                return Interactable.SecondaryInteractor.GlobalTransform.Basis.Y;
            case LookUpVector.Combined:
                return (Interactable.PrimaryInteractor.GlobalTransform.Basis.Y + Interactable.SecondaryInteractor.GlobalTransform.Basis.Y).Normalized();
        }

        return Vector3.Up;
    }
}

