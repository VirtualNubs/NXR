using Godot;
using NXR;
using NXRInteractable;
using System;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmRotatingBolt : Interactable
{

    [Export]
    private Vector3 _startRotation = Vector3.Zero;
    [Export]
    private Vector3 _endRotation = Vector3.Zero;

    [Export]
    private Vector3 _startPosition = Vector3.Zero;
    [Export]
    private Vector3 _endPosition = Vector3.Zero;

    private Transform3D _initTransform = new();

    private Transform3D _initGrab = new(); 

    // tool settings 
    [ExportGroup("Tool Settings")]
    [Export]
    private bool _setStartRotation = false;
    [Export]
    private bool _setStartPosition = false;
    [Export]
    private bool _setEndRotation = false;
    [Export]
    private bool _setEndPosition = false;
    [Export]
    private bool _goStartRotation = false;
    [Export]
    private bool _goEndRotation = false;
    [Export]
    private bool _goStartPosition = false;
    [Export]
    private bool _goEndPosition = false;

    private Transform3D _relativeGrab = new(); 

    private bool _setBack = false; 

    private Firearm _firearm = null; 

    [Signal]
    public delegate void OnBoltBackEventHandler();

    public override void _Ready()
    {
        base._Ready();
        _initTransform = Transform;
        OnGrabbed += OnGrab; 

        if (Util.NodeIs(GetParent(), typeof(Firearm))) { 
            _firearm = (Firearm)GetParent(); 
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Engine.IsEditorHint())
        {

            if (_setStartRotation)
            {
                _startRotation = Rotation;
                _setStartRotation = false;
            }
            if (_setEndRotation)
            {
                _endRotation = Rotation;
                _setEndRotation = false;
            }

            if (_goStartRotation)
            {
                Rotation = _startRotation;
                _goStartRotation = false;
            }
            if (_goEndRotation)
            {
                Rotation = _endRotation;
                _goEndRotation = false;
            }



            if (_setStartPosition)
            {
                _startPosition = Position;
                _setStartPosition = false;
            }
            if (_setEndPosition)
            {
                _endPosition = Position;
                _setEndPosition = false;
            }

            if (_goStartPosition)
            {
                Position = _startPosition;
                _goStartPosition = false;
            }
            if (_goEndPosition)
            {
                Position = _endPosition;
                _goEndPosition = false;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {

        if (PrimaryInteractor == null) return;

        Node3D parent = (Node3D)GetParent();
        Transform3D newXform = GetPrimaryRelativeXform();

        // Don't rotate if position not at start 
        if (Position.IsEqualApprox(_startPosition))
        {
            Transform3D xform = GlobalTransform * _relativeGrab; 
            Vector3 axis = Vector3.One; 
            Vector3 grab = ToLocal(GetPrimaryInteractor().GlobalPosition); 
            Vector3 loc = ToLocal(xform.Origin); 
            grab.Z = 0; 
            loc.Z = 0; 

            Vector3 grabDir = (grab - Position); 
            Vector3 locDir = (loc - Position);  

            float rotAngle = locDir.Normalized().SignedAngleTo(grabDir.Normalized(), axis);

            RotateZ(rotAngle);
        }

        // Isues with clamp if not deconsturected 
        Vector3 newPos = Position;
        Vector3 newRot = Rotation;
        newPos.X = _startPosition.X;
        newPos.Y = _startPosition.Y;
        newPos.Z = parent.ToLocal(newXform.Origin).Z;
        newPos.Z = Mathf.Clamp(newPos.Z, _startPosition.Z, _endPosition.Z);

        newRot = new Vector3(newRot.X, newRot.Y, Rotation.Z);
        newRot.Z = Mathf.Clamp(newRot.Z, _startRotation.Z, _endRotation.Z);

        if (!newRot.IsEqualApprox(_endRotation))
        {
            newPos.Z = Mathf.Clamp(newPos.Z, _startPosition.Z, _startPosition.Z);
        } 
        
        Rotation = newRot;
        Position = newPos;

        if (Position.IsEqualApprox(_endPosition)) { 
            _setBack = true; 
        }

        if (Position.IsEqualApprox(_startPosition) && _setBack) { 
            _setBack = false; 

            _firearm?.EmitSignal("TryChamber"); 
        }
    }

    public void OnGrab(Interactable interactable, Interactor interactor) { 
        if (interactor == PrimaryInteractor) { 
            _relativeGrab = GlobalTransform.AffineInverse() * interactor.GlobalTransform; 
        }
    }
}
