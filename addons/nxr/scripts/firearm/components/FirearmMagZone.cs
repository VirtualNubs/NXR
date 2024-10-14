using System.Security.Cryptography.X509Certificates;
using Godot;
using NXR;
using NXRFirearm;
using NXRInteractable;


namespace NXRFirearm;


[GlobalClass]
public partial class FirearmMagZone : InteractableSnapZone
{
    [Export] private bool _disableMag = false;

    [ExportGroup("Eject Settings")]
    [Export] public bool EjectEnabled = true; 
    [Export] private string _ejectAction = "ax_button";
    [Export] private float _ejectForce = 3f;


    
    public FirearmMag CurrentMag = null;
    public bool MagIn = false;


    private Firearm _firearm = null;


    [Signal] public delegate void TryEjectEventHandler();
    [Signal] public delegate void MagEnteredEventHandler(); 
    [Signal] public delegate void MagExitEventHandler(); 


    public override void _Ready()
    {

        OnSnap += OnSnapped;
        OnUnSnap += OnUnSnapped;
        TryEject += TriedEject; 

        base._Ready();

        _firearm = FirearmUtil.GetFirearmFromParentOrOwner(this);
        if (_firearm != null) { 
            _firearm.TryChamber += TryChamber;
            _firearm.OnGrabbed += FirearmGrabbed; 
            _firearm.OnFullDropped += FirearmDropped; 
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);


        if (_firearm == null || _firearm.GetPrimaryInteractor() == null) return; 
        
      
        if (_firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_ejectAction))
        {
            EmitSignal("TryEject");
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
        mag.InitParent = _firearm.InitParent;
        mag.PreviousParent = _firearm.InitParent;
        mag.InitFreeze = _firearm.InitFreeze; 
        
        
        if (!_firearm.IsGrabbed()) mag.Disabled = true; 
        EmitSignal("MagEntered"); 
    }


    private void OnUnSnapped()
    {
        if (CurrentMag != null) { 
            Interactable mag = (Interactable)CurrentMag;
            mag.Disabled = false;
            CurrentMag = null;
            EmitSignal("MagExit"); 
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

    private void TriedEject() { 
        if (CurrentMag == null || !EjectEnabled) return;

        Eject(CurrentMag); 
    }


    private void FirearmGrabbed(Interactable interactable, Interactor interactor) { 
        if (CurrentMag != null) { 
            CurrentMag.Disabled = false; 
        }
    }

    private void FirearmDropped() { 
         if (CurrentMag != null) { 
            CurrentMag.Disabled = true; 
        }
    }

    public Firearm GetFirearm()
    {
        return _firearm;
    }
}
