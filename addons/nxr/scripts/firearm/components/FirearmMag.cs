using Godot;
using System;
using NXRInteractable; 
using NXR;
using NXRFirearm;

namespace NXRFirearm; 

[GlobalClass]
public partial class FirearmMag : Interactable
{

    #region Exported
    [Export] private bool _internal = false; 
    [Export] private bool _infinite = false; 
    [Export] public int MaxAmmo = 30;
    [Export] public int CurrentAmmo;
    #endregion
    

    public bool CanChamber = true; 


    private Firearm _firearm; 



    public override void _Ready()
    {
        base._Ready(); 

        if (_internal && FirearmUtil.GetFirearmFromParentOrOwner(this) != null)
        {
            _firearm = FirearmUtil.GetFirearmFromParentOrOwner(this);
            _firearm.TryChamber += TryChamber; 
        }
    }

    private void TryChamber() { 
        if (!CanChamber) return; 
        
        if (_infinite) CurrentAmmo = 1; 

        if (CurrentAmmo <= 0) return; 
        _firearm.Chambered = true; 
        CurrentAmmo -= 1; 
        _firearm.EmitSignal("OnChambered"); 
    }


    public void RemoveBullet(int amount) { 
        CurrentAmmo -= amount; 
        CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, MaxAmmo); 
    }


    public void AddBullet(int amount) { 
        CurrentAmmo += amount; 
        CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, MaxAmmo); 
    }
}
