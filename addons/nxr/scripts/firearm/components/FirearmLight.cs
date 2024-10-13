using Godot;
using NXRFirearm;
using System;
using NXR;

[GlobalClass]
public partial class FirearmLight : OmniLight3D
{
    [Export] public float Energy = 1.0f;

    private Firearm _firearm;

    public override void _Ready()
    {
        if (_firearm == null && Util.NodeIs((Node3D)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
            _firearm.OnFire += OnFire; 
        }

        LightEnergy = 0.0f; 
    }

    private void OnFire() { 
        LightEnergy = Energy; 

        Tween tween = GetTree().CreateTween(); 
        tween.TweenProperty(this, "light_energy", 0.0, 0.05); 
    }
}
