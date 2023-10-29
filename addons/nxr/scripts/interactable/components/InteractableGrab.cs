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

    [Export]
    private float _rotationSmoothing = 20.0f;
    [Export]
    private float _positionSmoothing = 30.0f;

    private float _positionDelta = 0.0f; 
    private float _rotationDelta = 0.0f; 

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
        if (_usePhysics)
        {
            PhysicsGrab();
        }
        else
        {
            KinematicGrab((float)delta);
        }
    }

    public void OnGrab(Interactable interactable, Interactor interactor)
    {
        if (interactor == Interactable.PrimaryInteractor)
        {
            Interactable.Freeze = !_usePhysics;
            Interactable.LinearVelocity = Vector3.Zero;
            _positionDelta = 0.0f; 
        }

        _rotationDelta = 0.0f;
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        _rotationDelta = 0.0f;
        _perciseOffset = Vector3.Zero;

    }

    public void OnFullDrop()
    {

        Interactable.Freeze = _initFreezeState;
        Interactable.LinearDamp = _initLinearDamp;
        Interactable.AngularDamp = _initAngularDamp;
    }

    public Transform3D TwoHandXform()
    {

        Transform3D interactableXform = Interactable.GlobalTransform; 
        Transform3D lookXform = Interactable.PrimaryInteractor.GlobalTransform;
        Transform3D secondaryXform = Interactable.SecondaryInteractor.GlobalTransform;
        Vector3 up = Interactable.GlobalTransform.Basis.Y + GetUpVector();
        Vector3 lookDir = secondaryXform.Origin - Interactable.PrimaryInteractor.GlobalTransform.Origin;


        Interactable._primaryGrabTransorm.Basis = Interactable.Basis;
        lookXform.Basis = interactableXform.Basis.Slerp(Basis.LookingAt(lookDir.Normalized(), up.Normalized()).Orthonormalized(), _rotationDelta);
        return lookXform;
    }



    private void PhysicsGrab()
    {

        if (Interactable.GetPrimaryInteractor() == null) return;

        Interactor interactor = Interactable.GetPrimaryInteractor();
        Vector3 dir = interactor.Controller.GlobalPosition - Interactable.GlobalPosition;
        Quaternion currentRotation = Interactable.GlobalTransform.Basis.GetRotationQuaternion();
        Quaternion previousRotation = interactor.Controller.GlobalTransform.Basis.GetRotationQuaternion();
        Quaternion rotationChange = currentRotation * previousRotation.Inverse();
        Vector3 angularVelocity = rotationChange.Inverse().GetEuler();

        if (_percise)
        {
            dir = Interactable.GetPrimaryRelativeXform().Origin - Interactable.GlobalPosition;
        }

        if (Interactable.IsTwoHanded())
        {
            Quaternion current = TwoHandXform().Basis.GetRotationQuaternion();
            Quaternion prev = Interactable.GlobalTransform.Basis.GetRotationQuaternion();
            Quaternion change = current * prev.Inverse();
            angularVelocity = change.GetEuler();
        }


        float strength = 10;
        Interactable.LinearVelocity = dir * 10.0f * strength;
        Interactable.AngularVelocity = angularVelocity * 10.0f * strength;

    }

    private void KinematicGrab(float delta)
    {
        if (IsInstanceValid(Interactable.PrimaryInteractor))
        {
            Node3D grabPoint = Interactable.PrimaryGrabPoint;
            Interactor interactor = Interactable.PrimaryInteractor;
            Vector3 posOffset = Interactable.GlobalPosition - Interactable.PrimaryGrabPoint.GlobalPosition;
            Vector3 newPos = interactor.GlobalPosition + posOffset;
            Basis rotOffset = (Interactable.GlobalTransform.Basis.Inverse() * grabPoint.GlobalTransform.Basis).Orthonormalized();
            Basis newRot = (interactor.GlobalTransform.Basis * rotOffset).Orthonormalized();
            Basis lastBasis = _primaryXform.Basis.Orthonormalized();

            _rotationDelta = Mathf.Lerp(_rotationDelta, 1.0f, delta * _positionSmoothing);
            _positionDelta = Mathf.Lerp(_positionDelta, 1.0f, delta * _rotationDelta); 

            _primaryXform.Origin = Interactable.GlobalTransform.Origin.Slerp(newPos, _positionDelta);
            _primaryXform.Basis = Interactable.GlobalTransform.Basis.Slerp(newRot, _rotationDelta);

            if (_percise) { _primaryXform = Interactable.GetPrimaryRelativeXform(); }

            if (_twoHanded && Interactable.IsTwoHanded())
            {
                Interactable.GlobalTransform = TwoHandXform() * Interactable.GetOffsetXform();
            }
            else
            {
                Interactable.GlobalTransform = _primaryXform * Interactable.GetOffsetXform();
            }
        }

        if (IsInstanceValid(Interactable.SecondaryInteractor) && !IsInstanceValid(Interactable.PrimaryInteractor))
        {
            Interactable.GlobalTransform = Interactable.GetSecondaryRelativeXform();
        }
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

    private Vector3 GrabPointOffset()
    {
        return Interactable.PrimaryGrabPoint.GlobalPosition - Interactable.GlobalPosition;
    }
}


