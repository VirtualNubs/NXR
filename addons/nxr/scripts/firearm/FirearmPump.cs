using Godot;
using System;
using NXRFirearm;
using NXR;


[Tool]
[GlobalClass]
public partial class FirearmPump : FirearmMovable
{
    
    private Firearm _firearm; 
    private bool back = false;
    private bool _emptyShellIn = false; 

    public override void _Ready()
    {
         if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
            _firearm.OnFire += OnFire; 
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
        RunTool(); 

		if (_firearm == null) return;  

        bool locked = AtStart() && _firearm.Chambered;   

		if (AtEnd() && !back) {
            back = true;  
        }

        if(AtEnd() && _firearm.Chambered) { 
            _firearm.Chambered = false; 
            _firearm.EmitSignal("TryEject"); 
        }

        if (!AtEnd() && back) { 
            back = false; 
            _firearm.EmitSignal("TryChamber"); 
        }
        
        if (AtStart() && _firearm.Chambered) { 
            _firearm.BlockFire = false; 
        } else { 
            _firearm.BlockFire = true; 
        }

        if (_emptyShellIn && AtEnd()) { 
            _firearm.EmitSignal("TryEject"); 
            _emptyShellIn = false; 
        }
	}

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return; 

        if (_firearm.GetSecondaryInteractor() != null)  {
			Node3D parent = (Node3D)GetParent();
            Transform3D grabXform = _firearm.SecondaryInteractor.GlobalTransform; 
            Vector3 newPos = parent.ToLocal(grabXform.Origin);
            newPos= newPos.Clamp(GetMinOrigin(), GetMaxOrigin());
            Position = newPos;
		}
    }

    private void OnFire() { 
        _emptyShellIn = true; 
    }
}
