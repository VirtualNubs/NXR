using System.Data.Common;
using Godot;
using NXR;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmMagZoneTrack : FirearmMovable
{

	private bool _unsnapQueued = false; 
	public override void _Process(double delta)
	{
		RunTool();

		if (GetChild(0) == null) return;

		if (Util.NodeIs(GetChild(0), typeof(FirearmMagZone)))
		{
			FirearmMagZone zone = (FirearmMagZone)GetChild(0);

			if (zone._snappedInteractable != null && zone._snappedInteractable.GetPrimaryInteractor() != null)
			{

				Node3D parent = (Node3D)Target.GetParent();
				Vector3 grabPos = zone._snappedInteractable.GetPrimaryInteractor().Controller.GlobalPosition;
				Vector3 locGrab = parent.ToLocal(grabPos);
				Transform3D newXform = Transform;

				float distGrab = grabPos.DistanceTo(StartXform.Origin);
				newXform.Origin = locGrab;
				newXform.Origin = newXform.Origin.Clamp(GetMinOrigin(), GetMaxOrigin());

				if (distGrab < 0.4)
				{
					newXform.Origin = StartXform.Origin;
				}

				Transform = newXform;
			}
			else if (zone._snappedInteractable == null)
			{
				GD.Print("no"); 
				Transform = EndXform;
			}

			float distEnd = Transform.Origin.DistanceTo(EndXform.Origin);
			float distStart = Transform.Origin.DistanceTo(StartXform.Origin);

			if (distEnd <= 0.01)
			{
				zone.CanUnsnap = true;
				
				if (_unsnapQueued) {
					_unsnapQueued = false; 
					zone.Unsnap();
				}
			}
			else
			{
				zone.CanUnsnap = false;
				if (!_unsnapQueued) _unsnapQueued = true; 
			}

			if (distStart < 0.001) { 
				if (zone.CurrentMag != null) zone.CurrentMag.CanChamber = true; 
			} else{ 
				if (zone.CurrentMag != null) zone.CurrentMag.CanChamber = false; 
			}

			if (Position != StartXform.Origin && zone._snappedInteractable != null && zone._snappedInteractable.GetPrimaryInteractor() == null) { 
				Exit(zone); 
			}
		}
	}

	private async void Exit(InteractableSnapZone zone) { 
		Tween tween = GetTree().CreateTween(); 

		tween.TweenProperty(Target, "transform", EndXform, 0.2f); 

		await ToSignal(tween, "finished");

		zone.Unsnap(true); 
	}
}
