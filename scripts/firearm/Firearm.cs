using Godot;
using NXR; 

namespace NXRFirearm;

[GlobalClass]
public partial class Firearm : Interactable
{
    [Export]
    private FireMode _fireMode = FireMode.Single; 
    [Export]
    private float _fireRate = 0.1f;

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


    public bool IsChamnberd = false; 

    private Vector3 _initPositionOffset; 
    private Vector3 _initRotationOffset;
    private Timer _fireTimer = new();

    [Signal]
    public delegate void OnFireEventHandler(); 

    public override void _Ready() {
        AddChild(_fireTimer);
        _fireTimer.WaitTime = _fireRate;
        _fireTimer.OneShot = true;
    }

    public override void _Process(double delta)
    {
        if (CanFire() && GetFireInput())
        {
            Fire(); 
        }
        RecoilReturn(); 
    }

    public void Fire()
    {
        _fireTimer.Start();
        Recoil(); 
        EmitSignal("OnFire"); 
    }

    private bool CanFire()
    {
        return _fireTimer.IsStopped() && IsInstanceValid(PrimaryInteractor); 
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

        riseTween.TweenProperty(this, "RotationOffset", RotationOffset + _recoilRise * recoilMultiplier, 0.1);
        PositionOffset += _recoilKick * recoilMultiplier; 
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
        float positionLength = Mathf.Abs(_initPositionOffset.Length() - PositionOffset.Length()); 
        PositionOffset = PositionOffset.Lerp(_initPositionOffset, _kickRecoverSpeed);
        RotationOffset = RotationOffset.Lerp(_initRotationOffset, _riseRecoverSpeed);
    }
}
