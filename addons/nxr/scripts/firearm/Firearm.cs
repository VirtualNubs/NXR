using System;
using System.Runtime;
using System.Security.Cryptography;
using Godot;
using NXR;
using NXRInteractable;

namespace NXRFirearm;

[GlobalClass]
public partial class Firearm : Interactable
{

    #region Exported: 
    [Export] public FireMode FireMode { get; set; } = FireMode.Single;
    [Export] private int _roundPerMinute = 600;
    [Export] private bool _startChambered = false;
    [Export] private bool _chamberOnFire = true;


    [ExportGroup("Burst Settings")]
    [Export] private int _burstAmount = 3;
    [Export] private float _burstTime = 0.5f;


    [ExportGroup("TwoHanded Settings")]
    [Export] private float _recoilMultiplier = 0.3f;


    [ExportGroup("Recoil Settings")]
    [Export] private Vector3 _recoilKick = new Vector3(0, 0, 0.15f);
    [Export] private Vector3 _recoilRise = new Vector3(15, 0, 0);
    [Export] private float _recoilTimeToPeak = 0.01f;
    [Export] private float _kickRecoverSpeed = .25f;
    [Export] private float _riseRecoverSpeed = .25f;
    [Export] private Curve _yCurve;


    [ExportGroup("Haptic Settings")]
    [Export] private float _hapticStrength = 0.3f;
    #endregion


    #region Public: 
    public bool Chambered { get; set; } = false;
    public bool BlockFire { get; set; } = false;
    #endregion


    #region Private: 
    private bool _burstQueued = false;
    private Vector3 _initPositionOffset;
    private Vector3 _initRotationOffset;
    private Timer _fireTimer = new();
    private int _shotCount = 0;
    private Tween _recoilTween;
    #endregion


    #region Signals: 
    [Signal] public delegate void OnFireEventHandler();
    [Signal] public delegate void OnEjectEventHandler();
    [Signal] public delegate void OnChamberedEventHandler();
    [Signal] public delegate void TryChamberEventHandler();
    [Signal] public delegate void TryEjectEventHandler();
    [Signal] public delegate void TryEjectEmptyEventHandler();
    [Signal] public delegate void TryEjectSpentEventHandler();
    #endregion


    public override void _Ready()
    {
        base._Ready();

        _initPositionOffset = PositionOffset;
        _initRotationOffset = RotationOffset;

        // timer setup 
        AddChild(_fireTimer);
        _fireTimer.WaitTime =  60.0 / _roundPerMinute;
        _fireTimer.OneShot = true;
        _fireTimer.ProcessCallback = Timer.TimerProcessCallback.Physics;

        if (_startChambered)
        {
            Chambered = true;
        }

        OnEject += EjectChambered;
    }

    public override void _Process(double delta)
    {
        if (CanFire() && GetFireInput())
        {
            Fire(); 
        }
    }

    public  void Fire()
    {
        if (FireMode == FireMode.Burst) { 
            FireActionBurst(); 
        } else { 
            FireAction(); 
        }
    }

    private void FireAction() { 
        
        if (Util.NodeIs(GetParent(), typeof(InteractableSnapZone))) return;
        if (BlockFire || !Chambered) return;

        _shotCount += 1;
        Chambered = false;
        _fireTimer.Start();

        Recoil();
        GetPrimaryInteractor()?.Controller.Pulse(_hapticStrength, 1.0, 0.1);
        GetSecondaryInteractor()?.Controller.Pulse(_hapticStrength, 1.0, 0.1);

        if (_chamberOnFire) { EmitSignal("TryChamber"); }

        EmitSignal("OnFire");
    }


     public async void FireActionBurst()
    {

        _burstQueued = true;

        for (int i = 0; i < _burstAmount; i++)
        {
            FireAction(); 
            await ToSignal(GetTree().CreateTimer(60.0 / _roundPerMinute), "timeout");
        }

        _burstQueued = false;
    }

    private bool CanFire()
    {
        return _fireTimer.IsStopped() && IsInstanceValid(PrimaryInteractor) && Chambered;
    }

    private bool GetFireInput()
    {
        switch (FireMode)
        {
            case FireMode.Single:
                return PrimaryInteractor.Controller.ButtonOneShot("trigger_click");
            case FireMode.Burst:
                return PrimaryInteractor.Controller.ButtonOneShot("trigger_click") && !_burstQueued;
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
        float maxAngle = 90;


        if (_recoilTween == null) _recoilTween = GetTree().CreateTween(); 

        if (Mathf.Abs(RotationOffset.X) >= maxAngle) RotationOffset -= _recoilRise * 2.0f; // clamp rotatiion 
        if (SecondaryInteractor != null) recoilMultiplier = _recoilMultiplier;

        if (_yCurve != null)
        {
            _shotCount = _shotCount >= _yCurve.PointCount ? 0 : _shotCount;
            RotationOffset = new Vector3(RotationOffset.X, _yCurve.GetPointPosition(_shotCount).Y * recoilMultiplier, RotationOffset.Z);
        }

        if (_recoilTween.IsRunning())
        {
            _recoilTween.Kill();
        }

        _recoilTween = GetTree().CreateTween();
        _recoilTween.SetProcessMode(Tween.TweenProcessMode.Physics);

        _recoilTween.TweenProperty(this, "RotationOffset", RotationOffset + _recoilRise * recoilMultiplier, _recoilTimeToPeak / 2);
        _recoilTween.TweenProperty(this, "PositionOffset", PositionOffset + _recoilKick * recoilMultiplier, _recoilTimeToPeak / 2);


        _recoilTween.SetEase(Tween.EaseType.Out);
        _recoilTween.SetTrans(Tween.TransitionType.Spring);
        _recoilTween.TweenProperty(this, "PositionOffset", _initPositionOffset, _kickRecoverSpeed);

        _recoilTween.SetEase(Tween.EaseType.InOut);
        _recoilTween.Parallel().TweenProperty(this, "RotationOffset", _initRotationOffset * recoilMultiplier, _riseRecoverSpeed);
    }

   
    private void EjectChambered()
    {
        Chambered = false;
    }
}