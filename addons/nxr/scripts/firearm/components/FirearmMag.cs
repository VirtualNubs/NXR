using Godot;
using System;
using NXRInteractable; 
using NXR;
using NXRFirearm;

namespace NXRFirearm; 

[GlobalClass]
public partial class FirearmMag : Interactable
{
    [Export]
    private bool _internal = false; 
    [Export]
    private bool _infinite = false; 
    [Export]
    public int MaxAmmo = 30;

    [Export]
    public int CurrentAmmo;

    private Firearm _firearm; 

    public override void _Ready()
    {
        base._Ready(); 
        CurrentAmmo = MaxAmmo; 

         if (_internal && Util.NodeIs((Node)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
            _firearm.TryChamber += TryChamber; 
        }
    }

    private void Eject() { 
        
    }
    private void TryChamber() { 
        if (CurrentAmmo <= 0) return; 

        _firearm.Chambered = true; 
        CurrentAmmo -= 1; 
    }
}
