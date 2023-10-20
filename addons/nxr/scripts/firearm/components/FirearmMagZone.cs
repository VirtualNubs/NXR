using Godot;
using NXR;
using NXRFirearm;
using NXRInteractable;
using System;

[GlobalClass]
public partial class FirearmMagZone : InteractableSnapZone
{
    [Export]
    private Firearm _firearm = null; 
    public FirearmMag CurrentMag = null; 

    
    [Export]
    private string _dropAction = "ax_button"; 

    public override void _Ready()
    {
        OnSnap += OnSnapped; 
        OnUnSnap += OnUnSnapped;

        base._Ready();

        if (Util.NodeIs((Node)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }

        _firearm.TryChamber += TryChamber; 
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_firearm != null && _firearm.GetPrimaryInteractor() != null) { 
            if (_dropAction != null && _firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_dropAction)) { 
                Unsnap(); 
                _hoveredInteractable = null; 
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

    private void TryChamber() { 
        if (CurrentMag == null) return; 

        if (CurrentMag.CurrentAmmo > 0) { 
            _firearm.Chambered = true; 
            CurrentMag.CurrentAmmo -= 1; 
        }
    }
}
