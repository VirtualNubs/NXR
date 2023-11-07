using Godot;
using NXR;
using NXRInteractable;
using System;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmRotatingBolt : FirearmMovable
{

    private Transform3D _initTransform = new();

    private Transform3D _initGrab = new(); 

    private Transform3D _relativeGrab = new(); 

    private bool _setBack = false; 
    private float _rotationAngle = 0.0f; 

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
        RunTool();

         // chambering logic
        if (AtEnd()) { 
            _setBack = true; 
        }

        if (AtStart() && _setBack) { 
            _setBack = false; 
            _firearm?.EmitSignal("TryChamber"); 
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        
        if (Engine.IsEditorHint()) { 
            RunTool(); 
        }

        if (PrimaryInteractor == null) return;

        Node3D parent = (Node3D)GetParent();
        Transform3D newXform = GetPrimaryRelativeXform();

        // Don't rotate if position not at start 
        if (Position.IsEqualApprox(StartXform.Origin))
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
            rotAngle = Mathf.Clamp(rotAngle, -.1f, .1f); 
            RotateZ(rotAngle * 2.0f);
        }

        // rotation clamp
        Vector3 newRot = Rotation;
        Vector3 startEuler = StartXform.Basis.GetEuler(); 
        newRot = new Vector3(newRot.X, newRot.Y, Rotation.Z);
        newRot.Z = Mathf.Clamp(newRot.Z, GetMinRotation().Z, GetMaxRotation().Z);

        Rotation = newRot;

        // position clamp
        Vector3 newPos = Position;
        newPos.X = StartXform.Origin.X;
        newPos.Y = StartXform.Origin.Y;
        newPos.Z = parent.ToLocal(newXform.Origin).Z;
        newPos.Z = Mathf.Clamp(newPos.Z, GetMinOrigin().Z, GetMaxOrigin().Z);

        Vector3 endEuler = EndXform.Basis.GetEuler(); 
        if (!Rotation.IsEqualApprox(endEuler))
        {
            newPos.Z = Mathf.Clamp(newPos.Z, GetMinOrigin().Z, GetMinOrigin().Z);
        } 
        
        Position = newPos;

    }

    public void OnGrab(Interactable interactable, Interactor interactor) { 
        if (interactor == PrimaryInteractor) { 
            _relativeGrab = GlobalTransform.AffineInverse() * interactor.GlobalTransform; 
        }
    }
}
