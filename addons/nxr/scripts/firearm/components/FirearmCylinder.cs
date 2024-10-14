using Godot;
using NXR;
using NXRFirearm;
using System;

[Tool]
[GlobalClass]
public partial class FirearmCylinder : FirearmClampedXform
{
	[Export] private Firearm _firearm; 
	[Export]private Node3D _bulletQueue; 
	[Export] private Node3D _cylinderMesh; 
	[Export] private float _step; 


	private Vector3 newRotation; 

    public override void _Ready()
    {
        if (Util.NodeIs((Node)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }

		newRotation = _cylinderMesh.RotationDegrees; 
    }

    public override void _Process(double delta) { 
		
		if (Engine.IsEditorHint()) { 
			RunTool();
		}

		if (_firearm == null) return; 

		if (IsClosed() && GetOpenInput()) { 
			Open(); 
		}

		if (!IsClosed() && GetCloseInput()) { 
			Close(); 
		}

		if (!IsClosed()) { 
			_firearm.BlockFire = true; 
		} else{ 
			_firearm.BlockFire = false; 
		}


		// bullet eject
		if(_bulletQueue != null) { 
			if (!IsClosed() && -GlobalTransform.Basis.Z.Dot(Vector3.Up) > 0.8) { 
				FirearmBulletZoneQueue queue = (FirearmBulletZoneQueue)_bulletQueue; 
				queue.EjectAll(Vector3.Zero,  Vector3.Zero, true); 
			}
		}

		float triggerValue = _firearm.GetTriggerValue(); 
		
	}

	private bool IsClosed() { 
		return Target.Transform.IsEqualApprox(StartXform); 
	}
	

	public void Open() { 
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Target, "transform", EndXform, 0.1f); 
	}

	public void Close() { 
		Tween tween = GetTree().CreateTween(); 
		tween.TweenProperty(Target, "transform", StartXform, 0.05f); 
	}
	
	private bool GetOpenInput() { 
		if (_firearm.GetPrimaryInteractor() == null) return false; 
		bool input = _firearm.GetPrimaryInteractor().Controller.IsButtonPressed("by_button"); 
		Controller controller = _firearm.GetPrimaryInteractor().Controller; 
		Vector3 dir = -_firearm.GetPrimaryInteractor().Controller.Transform.Basis.X;

		return controller.VelMatches(dir, 1f) && input; 
	}

	private bool GetCloseInput() { 
		if (_firearm.GetPrimaryInteractor() == null) return false; 
		
		Controller controller = _firearm.GetPrimaryInteractor().Controller; 
		Vector3 dir = _firearm.GetPrimaryInteractor().Controller.Transform.Basis.X;

		return controller.VelMatches(dir, 1f); 
	}
}
