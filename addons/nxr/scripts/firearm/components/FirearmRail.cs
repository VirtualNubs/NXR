using Godot;
using NXR;
using System;


[GlobalClass]
public partial class FirearmRail : InteractableSnapZone
{
	[Export]
	private float _snap = 0.01f; 

	[Export]
	private float _railLength = 0.2f;  

	[Export]
	private float _minZPosition = 0; 

	[Export]
	private float _maxZPosition = 0; 

	private Vector3 _initPosition; 

	float prevZ = 0f; 
    public override void _Ready()
    {
        base._Ready();

		_initPosition = Position; 
		OnUnSnap += UnSnapped; 
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		base._Process(delta); 
			
		if (_snappedInteractable != null && _snappedInteractable.IsInsideTree()) { 

			Node3D parent = (Node3D)GetParent(); 
			Transform3D newXform = _snappedInteractable.GlobalTransform; 
			Transform3D railXform = GlobalTransform;

			newXform.Basis = GlobalTransform.Basis; 
			newXform.Origin = railXform.Origin; 

			if (_snappedInteractable.GetPrimaryInteractor() != null) { 
				Vector3 newPos = parent.ToLocal(_snappedInteractable.GetPrimaryInteractor().Controller.GlobalPosition); 
				float halfLength = _railLength / 2; 

				newPos.X = Position.X; 
				newPos.Y = Position.Y;
				newPos.Z = Mathf.Snapped(newPos.Z, _snap); 
				newPos.Z = Mathf.Clamp(newPos.Z, _initPosition.Z - halfLength, _initPosition.Z + halfLength); 


				if (newPos.Z != Position.Z) { 
					_snappedInteractable.GetPrimaryInteractor().Controller.Pulse(1.0, 0.05, 0.1); 
				}

				Position = newPos; 
			}

			_snappedInteractable.GlobalTransform = newXform; 

		}

	}

    private void UnSnapped() { 
		Position = _initPosition; 
	}
}
