using System;
using Godot;
using NXRInteractable; 

namespace NXRFirearm;

[GlobalClass]
public partial class Firearm : Interactable
{
    [Export]
    private FireMode _fireMode = FireMode.Single; 
    [Export]
    private float _fireRate = 0.1f;
    

    [Export]
    private bool _startChambered = false; 
    [Export]
    private bool _chamberOnFire = true; 


    [ExportGroup("TwoHanded Settings")]
    [Export]
    private float _recoilMultiplier = 0.3f;  

    [ExportGroup("Recoil Settings")]
    [Export]
    private Vector3 _recoilKick = new Vector3(0, 0, 0.01f);
    [Export]
    private Vector3 _recoilRise = new Vector3(15, 0, 0);
    [Export]
    private float _kickRecoverSpeed = 0.1f;
    [Export]
    private float _riseRecoverSpeed = 0.1f;

    [Export]
    private Curve _yCurve; 

    public bool Chambered = false; 

    private Vector3 _initPositionOffset; 
    private Vector3 _initRotationOffset;
    private Timer _fireTimer = new();
    private int _shotCount = 0; 

    [ExportGroup("Haptic Settings")]
    [Export]
    private float _hapticStrength = 0.5f; 


    public bool BlockFire; 

    [Signal]
    public delegate void OnFireEventHandler(); 

    [Signal]
    public delegate void TryChamberEventHandler(); 

    [Signal]
    public delegate void OnChamberdEventHandler(); 


    public override void _Ready() {
        base._Ready(); 

        _initPositionOffset = PositionOffset; 
        _initRotationOffset = RotationOffset; 
        AddChild(_fireTimer);
        _fireTimer.WaitTime = _fireRate;
        _fireTimer.OneShot = true;

        if (_startChambered) { 
            Chambered = true; 
        }
    }

    public override void _Process(double delta)
    {
        if (CanFire() && GetFireInput())
        {
            Fire(); 
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsGrabbed()) {
            RecoilReturn(); 
        }
    }
    public void Fire()
    {   
        if (BlockFire) return; 
        
        _shotCount += 1; 
        Chambered = false; 
        _fireTimer.Start();
        Recoil(); 

        
        GetPrimaryInteractor()?.Controller.Pulse(_hapticStrength, 1.0, 0.1);
        GetSecondaryInteractor()?.Controller.Pulse(_hapticStrength, 1.0, 0.1);

        if (_chamberOnFire) { EmitSignal("TryChamber"); }
        EmitSignal("OnFire"); 
    }

    private bool CanFire()
    {
        return _fireTimer.IsStopped() && IsInstanceValid(PrimaryInteractor) && Chambered; 
    }

    private bool GetFireInput()
    {
        switch (_fireMode)
        {
            case FireMode.Single:
                return PrimaryInteractor.Controller.ButtonOneShot("trigger_click");
            case FireMode.Burst:
                return PrimaryInteractor.Controller.ButtonOneShot("trigger_click");
            case FireMode.Auto:
                return PrimaryInteractor.Controller.GetFloat("trigger_click") > 0.5;
        }
        return false;
    }

    public float GetTriggerValue()
    {
        if (PrimaryInteractor != null)
        {
            return PrimaryInteractor.Controller.GetFloat("trigger"); 
        }
        return 0.0f; 
    }

    private void Recoil()
    {

        float recoilMultiplier = 1.0f; 


        if (SecondaryInteractor != null) 
        {
            recoilMultiplier = _recoilMultiplier;
        }

        Tween riseTween = GetTree().CreateTween();
        
        if (_yCurve != null) {
            float sampled = _yCurve.SampleBaked(Mathf.Cos(_shotCount ) / _yCurve.BakeResolution); 
            RotationOffset.Y = sampled; 
        }

        riseTween.TweenProperty(this, "RotationOffset", RotationOffset + _recoilRise * recoilMultiplier, 0.1);
        PositionOffset += _recoilKick * _recoilMultiplier; 
        ApplyCentralImpulse(GlobalTransform.Basis.Z); 
        ApplyTorqueImpulse(Basis.X); 
    }

    public async void BurstFire()
    {
        Fire(); 
        await ToSignal(GetTree().CreateTimer(0.2), "timeout"); 
        Fire();
        await ToSignal(GetTree().CreateTimer(0.2), "timeout");
        Fire();
    }

    private void RecoilReturn()
    {
        float recoverMultiplier = 1.0f; 

        if (SecondaryInteractor == null) { 
            recoverMultiplier = 0.5f; 
        }

        float positionLength = Mathf.Abs(_initPositionOffset.Length() - PositionOffset.Length()); 
        PositionOffset = PositionOffset.Lerp(_initPositionOffset, _kickRecoverSpeed * recoverMultiplier);
        RotationOffset = RotationOffset.Lerp(_initRotationOffset, _riseRecoverSpeed * recoverMultiplier);
    }
}
