using Godot;
using System;
using NXR;
using NXRFirearm;


[GlobalClass]
public partial class FirearmSpawner : Node3D
{   
    [Export] private PackedScene _scene; 
    private Firearm _firearm; 

    public override void _Ready()
    {
        _firearm = FirearmUtil.GetFirearmFromParentOrOwner(this); 
        if (_firearm != null)
        {
            _firearm = (Firearm)GetParent();
            _firearm.OnFire += OnFire; 
            _firearm.TryEject += OnEject; 
        }
    }

    private void OnFire() { 
        FirearmBullet inst = _scene.Instantiate<FirearmBullet>(); 
        
        _firearm.GetParent().AddChild(inst); 
        inst.GlobalPosition = GlobalPosition; 

        float randX = GD.Randf(); 
        float randY = GD.Randf(); 
        float randZ = GD.Randf(); 
        
        inst.Rotation = new Vector3(randX, randY, randZ);
        inst.UpdateTip(true);

        if (Util.NodeIs(inst, typeof(RigidBody3D))) { 
            RigidBody3D body = (RigidBody3D)inst; 
            body.ApplyCentralForce(GlobalTransform.Basis.X * 30); 
        }
    }

    private void OnEject() {
        FirearmBullet inst = _scene.Instantiate<FirearmBullet>(); 
        
        _firearm.GetParent().AddChild(inst); 
        inst.GlobalPosition = GlobalPosition; 

        float randX = GD.Randf(); 
        float randY = GD.Randf(); 
        float randZ = GD.Randf(); 
        
        inst.Rotation = new Vector3(randX, randY, randZ); 
        inst.UpdateTip(false);

        if (Util.NodeIs(inst, typeof(RigidBody3D))) { 
            RigidBody3D body = (RigidBody3D)inst; 
            body.ApplyCentralForce(GlobalTransform.Basis.X * 30); 
        }
    }
}
