using Godot;
using NXR;
using NXRInteractable;
using System;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmRotatingBolt : FirearmMovable
{

    #region Private 
    private Transform3D _relativeGrab = new(); 
    private bool _setBack = false; 
    private float _rotationAngle = 0.0f; 
    private Firearm _firearm = null; 
    private float _lerpSpeed = 0.5f; 
    #endregion


    #region Signals
    [Signal] public delegate void OnBoltBackEventHandler();
    [Signal] public delegate void OnBoltForwardEventHandler();
    #endregion

    

    public override void _Ready()
    {
        base._Ready();
        OnGrabbed += OnGrab; 

        _firearm = FirearmUtil.GetFirearmFromParentOrOwner(this); 
    }


    public override void _Process(double delta)
    {
        base._Process(delta);   
        RunTool();

         // chambering logic
        if (AtEnd() && !_setBack) { 
            _setBack = true; 
            EmitSignal("OnBoltBack"); 
        }

        if (AtStart() && _setBack) { 
            _setBack = false; 
            _firearm?.EmitSignal("TryChamber"); 
            EmitSignal("OnBoltForward"); 
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
            Vector3 loc = ToLocal(xform.Origin); 
            Vector3 grab = ToLocal(GetPrimaryInteractor().GlobalPosition); 
            grab.Z = 0;
            loc.Z = 0; 

            grab = grab.LimitLength(loc.Length()); 

            Vector3 locDir = (loc - Position).Normalized();  
            Vector3 grabDir = (grab - Position).Normalized(); 
            float newAngle = locDir.SignedAngleTo(grabDir, axis);

            _rotationAngle = Mathf.Lerp(_rotationAngle, newAngle, _lerpSpeed); 

            RotateZ(_rotationAngle);    
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
