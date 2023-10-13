using Godot;
using System;
using NXRFirearm; 

[Tool]
public partial class FirearmPump : FirearmSlide
{
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		base._Process(delta); 

		if (_firearm == null) return;  

		if (_firearm.GetSecondaryInteractor() != null)  {
			Node3D parent = (Node3D)GetParent();
            Transform3D grabXform = _firearm.GetSecondaryRelativeXform(); 
            Vector3 newPos = parent.ToLocal(grabXform.Origin);

            newPos= newPos.Clamp(_startPosition, _endPosition);
            Position =  newPos;
		}

		if (IsBack() && !back && _firearm.GetSecondaryInteractor != null) {
            back = true;  
        }

        if (IsForward() && back) { 
            back = false; 
            _firearm.EmitSignal("TryChamber"); 
        }
	}
}
