using Godot;
using NXR;
using NXRInteractable;
using System;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmRotatingBolt : FirearmClampedXform
{

    [Export] private float _rotationStrength = 10.0f;
    [Export] private float _pullStrength = 10.0f;


    #region Private 
    private Transform3D _relativeGrab = new();
    private Vector3 _prevLocalGrab = new(); 
    private bool _setBack = false;
    private float _rotationAngle = 0.0f;
    private float _lerpSpeed = 0.5f;
    private float _prevPull; 
    private float _prevRot; 
    private float _rotOffset; 
    private float _pullOffset; 
    #endregion


    #region Signals
    [Signal] public delegate void OnBoltBackEventHandler();
    [Signal] public delegate void OnBoltForwardEventHandler();
    #endregion


    public override void _Ready()
    {
        base._Ready();

        if (Firearm != null)
        {
            OnGrabbed += OnGrab;
            OnDropped += OnDrop;
        }
        GD.Print(Firearm); 
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
        RunTool();

        // chambering logic
        if (AtEnd() && !_setBack)
        {
            _setBack = true;
            EmitSignal("OnBoltBack");
        }

        if (AtStart() && _setBack)
        {
            _setBack = false;
            Firearm?.EmitSignal("TryChamber");
            EmitSignal("OnBoltForward");
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        RunTool();

        if (PrimaryInteractor == null) return;

        Node3D parent = (Node3D)GetParent();
        Transform3D newXform = GetPrimaryRelativeXform();

        // Don't rotate if position not at start 
        Vector3 localGrab = Firearm.ToLocal(GetPrimaryInteractor().GlobalPosition); 

        if (Position.IsEqualApprox(StartXform.Origin))
        {
            _rotOffset = _prevRot + (localGrab - _prevLocalGrab).Y * _rotationStrength; 
            _rotOffset = Mathf.Clamp(_rotOffset, 0, 1); 

            Basis = StartXform.Basis.Orthonormalized().Slerp(EndXform.Basis.Orthonormalized(), _rotOffset); 
        }
        

        if (Basis.Orthonormalized().IsEqualApprox(EndXform.Basis.Orthonormalized()))
        {
            _pullOffset =  _prevPull + (localGrab - _prevLocalGrab).Z * _pullStrength;
            _pullOffset = Mathf.Clamp(_pullOffset, 0, 1); 
            

            Position = StartXform.Origin.Lerp(EndXform.Origin, _pullOffset); 
        }
    }


    public void OnGrab(Interactable interactable, Interactor interactor)
    {
        if (interactor == PrimaryInteractor)
        {
            _relativeGrab = GlobalTransform.AffineInverse() * interactor.GlobalTransform;
            _prevLocalGrab = Firearm.ToLocal(interactor.GlobalPosition); 
        }
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        _prevRot = _rotOffset; 
        _prevPull = _pullOffset; 
    }
}
