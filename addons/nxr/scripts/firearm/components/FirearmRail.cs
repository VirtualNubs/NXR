using Godot;
using NXR;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;


[GlobalClass]
public partial class FirearmRail : Node3D
{
	[Export] private float _snap = 0.01f;
	[Export] private float _railLength = 0.2f;
	[Export] private float _seperation = 0.05f; 

	private List<InteractableSnapZone> _zones = new List<InteractableSnapZone>();

	private Vector3 _initPosition;
	float prevZ = 0f;


	public override void _Ready()
	{
		base._Ready();

		_initPosition = Position;


		int index = 0; 
		foreach (Node child in GetChildren())
		{
			if (Util.NodeIs(child, typeof(InteractableSnapZone)))
			{
				InteractableSnapZone zone = (InteractableSnapZone)child;
				_zones.Add((InteractableSnapZone)child);

				Vector3 newPos = Vector3.Zero; 

				newPos.Z = _seperation * index; 
				zone.Position = newPos; 
			}

			index += 1; 
		}
	}



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		foreach (InteractableSnapZone zone in _zones)
		{

			if (zone.SnappedInteractable != null && zone.SnappedInteractable.IsInsideTree())
			{

				Node3D parent = (Node3D)GetParent();
				Transform3D newXform = zone.SnappedInteractable.GlobalTransform;
				Transform3D railXform = GlobalTransform;

				newXform.Basis = GlobalTransform.Basis;
				newXform.Origin = railXform.Origin;

				if (zone.SnappedInteractable.GetPrimaryInteractor() != null)
				{
					Vector3 newPos = parent.ToLocal(zone.SnappedInteractable.GetPrimaryInteractor().Controller.GlobalPosition);
					float halfLength = _railLength / 2;

					newPos.X = Position.X;
					newPos.Y = Position.Y;
					newPos.Z = Mathf.Snapped(newPos.Z, _snap);
					newPos.Z = Mathf.Clamp(newPos.Z, _initPosition.Z - halfLength, _initPosition.Z + halfLength);


					if (newPos.Z != Position.Z)
					{
						zone.SnappedInteractable.GetPrimaryInteractor().Controller.Pulse(1.0, 0.05, 0.1);
					}


					foreach (InteractableSnapZone other in _zones) { 
						if (other != zone) { 
							if (Mathf.Abs(newPos.Z - other.Position.Z) < _seperation ) { 
								return; 
							}
						}
					}
					
					newPos.Y = 0.0f; 
					zone.Position = newPos;
				}
			}
		}
	}


	private bool ValidDistance(InteractableSnapZone zone, Vector3 newPos)
	{
		foreach (InteractableSnapZone other in _zones)
		{
			if (other == zone) return true; 

			float dist = Mathf.Abs((other.Position.Z + zone.Position.Z)  - newPos.Z); 

			return dist > 0.1f; 

		}

		return false;
	}
}
