using Godot;
using System;
using NXRFirearm;
using NXR;


[Tool]
public partial class FirearmPump : FirearmMovable
{
    
    private Firearm _firearm; 
    private bool back = false;


    public override void _Ready()
    {
         if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        RunTool(); 

		if (_firearm == null) return;  

		if (_firearm.GetSecondaryInteractor() != null && !_firearm.Chambered)  {
			Node3D parent = (Node3D)Target.GetParent();
            Transform3D grabXform = _firearm.SecondaryInteractor.GlobalTransform; 
            Vector3 newPos = parent.ToLocal(grabXform.Origin);

            newPos= newPos.Clamp(GetMinOrigin(), GetMaxOrigin());
            
            Target.Position = newPos;
		}

		if (IsBack() && !back && _firearm.GetSecondaryInteractor != null) {
            back = true;  
        }

        if (IsForward() && back) { 
            back = false; 
            _firearm.EmitSignal("TryChamber"); 
        }
	}

    public bool IsForward() { 
        return Target.Transform.IsEqualApprox(StartXform); 
    }
    public bool IsBack() { 
        return Target.Transform.IsEqualApprox(EndXform); 
    }
}
