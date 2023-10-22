using Godot;
using System;
using NXR;
using NXRFirearm;


[GlobalClass]
public partial class FirearmSpawner : Node3D
{   
    [Export]
    private PackedScene _scene; 
    private Firearm _firearm; 

    public override void _Ready()
    {
        if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
            _firearm.OnFire += OnFire; 
        }
    }

    private void OnFire() { 
        Node3D inst = (Node3D)_scene.Instantiate(); 
        
        _firearm.GetParent().AddChild(inst); 
        inst.GlobalPosition = GlobalPosition; 

        float rand = GD.Randf(); 
        inst.Rotation = new Vector3(rand, rand, rand); 

        if (Util.NodeIs(inst, typeof(RigidBody3D))) { 
            RigidBody3D body = (RigidBody3D)inst; 
            GD.Print("force"); 
            body.ApplyCentralForce(GlobalTransform.Basis.X * 30); 
        }
    }
}
