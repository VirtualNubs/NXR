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
    private string _ejectAction = "ax_button"; 
    public bool MagIn = false; 

    public override void _Ready()
    {
        OnSnap += OnSnapped; 
        OnUnSnap += OnUnSnapped;

        base._Ready();

        if (Util.NodeIs((Node)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }

        if (_firearm != null) 
        {
            _firearm.TryChamber += TryChamber; 
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_firearm != null && _firearm.GetPrimaryInteractor() != null) { 
            if (_ejectAction != null && _firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_ejectAction) && CurrentMag != null) { 
                FirearmMag mag = CurrentMag; 
                Eject(CurrentMag); 
            }
        }
    }

    private void OnSnapped(Interactable mag) { 
        if (!Util.NodeIs(mag, typeof(FirearmMag))) return; 
        CurrentMag = (FirearmMag)mag; 
        mag.InitParent = (Node3D)Owner.GetParent(); 
    }

    private void OnUnSnapped() { 
        CurrentMag = null; 
    }

    private void Eject(FirearmMag mag) { 
        Unsnap(); 
        mag.ApplyCentralImpulse(mag.GlobalTransform.Basis.Z); 
    }

    private void TryChamber() { 
        if (CurrentMag == null) return; 

        if (!CurrentMag.CanChamber) return; 
        
        if (CurrentMag.CurrentAmmo > 0) { 
            _firearm.Chambered = true; 
            CurrentMag.RemoveBullet(1); 
            _firearm.EmitSignal("OnChambered"); 
        }
    }
}
