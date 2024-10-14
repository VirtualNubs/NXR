using Godot;
using NXR;

namespace NXRFirearm;


[GlobalClass]
public partial class FirearmRay : RayCast3D
{
    
    [Export] private  Firearm _firearm;

    [Signal] public delegate void OnHitEventHandler(Node3D node, Vector3 at); 


    public override void _Ready()
    {

        if (_firearm == null) { 
            _firearm = FirearmUtil.GetFirearmFromParentOrOwner(this); 
        }
        
        if (IsInstanceValid(_firearm))
        {
            _firearm.OnFire += OnFire; 
        }
    }

    private void OnFire()
    {
        if (!IsInstanceValid(GetCollider())) return; 

        EmitSignal("OnHit", GetCollider(), GetCollisionPoint()); 
        if (GetCollider().HasMethod("hit")) { 
            GetCollider().Call("hit", GetCollider(), GetCollisionPoint()) ; 
        }

        if (GetCollider().HasMethod("apply_impulse")) { 
            RigidBody3D body = (RigidBody3D)GetCollider(); 
            body.ApplyCentralImpulse(-GlobalTransform.Basis.Z); 
        }
    }
}
