using Godot;
using Godot.NativeInterop;
using NXR;
using NXRInteractable;

[GlobalClass]
public partial class InteractableGrab : Node
{
    [Export]
    private bool _usePhysics = false; 


    [Export]
    private bool _percise = false;

    [ExportGroup("TwoHandSettings")]
    [Export]
    private bool _twoHanded = false;
    [Export]
    private LookUpVector _lookUpVector = LookUpVector.PrimaryInteractor;
    [Export]
    private bool invert = false; 


    [ExportGroup("Physics Grab Settings")]

    private float _initLinearDamp = 0.0f; 
    private float _initAngularDamp = 0.0f; 



    private Vector3 _perciseOffset = new();
    private bool _initFreezeState = false;

    public Interactable Interactable;
    private Transform3D _primaryXform = new(); 


	private Vector3 lVelocity = Vector3.Zero; 
	private Vector3 aVelocity = Vector3.Zero; 
    public override void _Ready()
    {
        if (Interactable == null && Util.NodeIs((Node3D)GetParent(), typeof(Interactable)))
        {
            Interactable = (Interactable)GetParent();
            Interactable.OnGrabbed += OnGrab;
            Interactable.OnDropped += OnDrop;
            Interactable.OnFullDropped += OnFullDrop;
            _initFreezeState = Interactable.Freeze;
            _initLinearDamp = Interactable.LinearDamp; 
            _initAngularDamp = Interactable.AngularDamp; 
        }

    }

    public override void _PhysicsProcess(double delta)
    {   
        if (_usePhysics) { 
            PhysicsGrab(); 
        } else { 
            KinematicGrab(); 
        }
    }

    public void OnGrab(Interactable interactable, Interactor interactor)
    {
        if (interactor == Interactable.PrimaryInteractor)
        {
            Interactable.Freeze = !_usePhysics;
            Interactable.LinearVelocity = Vector3.Zero;
        }
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        _perciseOffset = Vector3.Zero;
    }

    public void OnFullDrop() { 

        Interactable.Freeze = _initFreezeState;
        Interactable.LinearDamp = _initLinearDamp; 
        Interactable.AngularDamp = _initAngularDamp; 
    }

    public Transform3D TwoHandXform()
    {

        Transform3D lookXform = Interactable.PrimaryInteractor.Controller.GlobalTransform;
        Transform3D secondaryXform = Interactable.SecondaryInteractor.Controller.GlobalTransform;
        Vector3 up = Interactable.GlobalTransform.Basis.Y + GetUpVector();
        Vector3 lookDir = secondaryXform.Origin - Interactable.PrimaryInteractor.GlobalTransform.Origin;
                

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

    private void PhysicsGrab() { 

        if (Interactable.GetPrimaryInteractor() == null) return; 

        Interactor interactor = Interactable.GetPrimaryInteractor(); 

		float distSqr = interactor.Controller.GlobalPosition.DistanceSquaredTo(Interactable.GlobalPosition);
		Vector3 dir = interactor.Controller.GlobalPosition - Interactable.GlobalPosition;
		Quaternion currentRotation = Interactable.GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion previousRotation = interactor.Controller.GlobalTransform.Basis.GetRotationQuaternion();
		Quaternion rotationChange = currentRotation * previousRotation.Inverse();
		Vector3 angularVelocity = rotationChange.Inverse().GetEuler();

        if (_percise) { 
            dir = Interactable.GetPrimaryRelativeXform().Origin - Interactable.GlobalPosition; 
        }

        if (Interactable.IsTwoHanded()) { 
            Quaternion current = TwoHandXform().Basis.GetRotationQuaternion(); 
            Quaternion prev = Interactable.GlobalTransform.Basis.GetRotationQuaternion(); 
            Quaternion change = current * prev.Inverse();
            angularVelocity = change.GetEuler(); 
        }

		Interactable.LinearDamp = 30;
		Interactable.AngularDamp = 20;
		Interactable.ApplyCentralForce(dir * 10000);
		Interactable.ApplyTorque(angularVelocity * 30);

    }

    private void KinematicGrab() { 
         if (IsInstanceValid(Interactable.PrimaryInteractor))
        {
            _primaryXform = Interactable.PrimaryInteractor.Controller.GlobalTransform;
            _primaryXform.Basis = _primaryXform.Basis;
            _primaryXform.Origin = _primaryXform.Origin;

            if (_percise) { _primaryXform = Interactable.GetPrimaryRelativeXform(); }
            
            if (!_twoHanded)
            {
                Interactable.GlobalTransform = _primaryXform * Interactable.GetOffsetXform();
            }
            
            if (Interactable.IsTwoHanded())
            {
                Interactable.GlobalTransform = TwoHandXform() * Interactable.GetOffsetXform();
            }
        }

        if (IsInstanceValid(Interactable.SecondaryInteractor) && !IsInstanceValid(Interactable.PrimaryInteractor)) { 
            Interactable.GlobalTransform = Interactable.GetSecondaryRelativeXform(); 
        }
    }
}

