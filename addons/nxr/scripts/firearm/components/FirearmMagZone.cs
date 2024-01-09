using Godot;
using NXR;
using NXRFirearm;
using NXRInteractable;

namespace NXRFirearm;

[GlobalClass]
public partial class FirearmMagZone : InteractableSnapZone
{
    [Export]
    private Firearm _firearm = null;
    public FirearmMag CurrentMag = null;

    [Export]
    private bool _disableMag = false;

    [ExportGroup("Eject Settings")]
    [Export]
    private string _ejectAction = "ax_button";
    [Export]
    private float _ejectForce = 3f;

    public bool MagIn = false;


    [Signal]
    public delegate void OnEjectEventHandler();

    public override void _Ready()
    {
        OnSnap += OnSnapped;
        OnUnSnap += OnUnSnapped;

        base._Ready();

        if (Util.NodeIs((Node)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
            _firearm.TryChamber += TryChamber;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_firearm != null && _firearm.GetPrimaryInteractor() != null)
        {
            if (_ejectAction != null && _firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_ejectAction) && CurrentMag != null)
            {
                CurrentMag.LinearVelocity = Vector3.Zero; 
                Eject(CurrentMag);
                EmitSignal("OnEject");
            }
        }
    }

    private void OnSnapped(Interactable mag)
    {
        if (!Util.NodeIs(mag, typeof(FirearmMag))) return;

        if (_disableMag)
        {
            mag.FullDrop(); 
            mag.Disabled = true;
        }

        CurrentMag = (FirearmMag)mag;
        mag.InitParent = (Node3D)Owner.GetParent();
    }

    private void OnUnSnapped()
    {
        if (CurrentMag != null) { 
            Interactable mag = (Interactable)CurrentMag;
            mag.Disabled = false;
            CurrentMag = null;
        }
    }

    private void Eject(FirearmMag mag)
    {

        Unsnap();

        if (GetFirearm() != null && GetFirearm().PrimaryInteractor != null)
        {
            Vector3 linear = GetFirearm().PrimaryInteractor.Controller.GetGlobalVelocity();
            float linearLength = GetFirearm().PrimaryInteractor.Controller.GetLocalVelocity().Length();
   
            Vector3 anguler = GetFirearm().PrimaryInteractor.Controller.GetAngularVelocity();
            float angLength = anguler.Normalized().Length();

            mag.LinearVelocity = (GetFirearm().PrimaryInteractor.Controller.GetGlobalVelocity().Normalized() * linearLength) * angLength * _ejectForce;
            mag.AngularVelocity = anguler;
        }
    }


    private void TryChamber()
    {
        if (CurrentMag == null) return;

        if (!CurrentMag.CanChamber) return;

        if (CurrentMag.CurrentAmmo > 0)
        {
            _firearm.Chambered = true;
            CurrentMag.RemoveBullet(1);
            _firearm.EmitSignal("OnChambered");
        }
    }

    public Firearm GetFirearm()
    {
        return _firearm;
    }
}
