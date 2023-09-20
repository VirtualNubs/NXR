using Godot;

namespace NXRFirearm;

[GlobalClass]
public partial class FirearmRay : RayCast3D
{
    [Export]
    private Firearm _firearm;

    [Signal]
    public delegate void OnHitEventHandler(Node3D node, Vector3 at); 

    public override void _Ready()
    {
        if (IsInstanceValid(_firearm))
        {
            _firearm.OnFire += OnFire; 
        }
    }

    private void OnFire()
    {
        if (IsInstanceValid(GetCollider())) {
            EmitSignal("OnHit", GetCollider(), GetCollisionPoint()); 
        }
    }
}
