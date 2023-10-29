using System;
using System.Data.Common;
using Godot;
using NXR;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmMagZoneTrack : FirearmMovable
{

	private bool _unsnapQueued = false;

	[Export]
	private float pullStrength = 2.0f; 
	float t = 0.0f;


    public override void _Ready()
    {
        if (!Util.NodeIs(GetChild(0), typeof(FirearmMagZone))) return;

		FirearmMagZone zone = (FirearmMagZone)GetChild(0);

		if (zone.CurrentMag != null) { 
			Transform = StartXform; 
		}
    }
    public override void _Process(double delta)
	{
		RunTool();

		if (GetChild(0) == null) return;
		
		if (!Util.NodeIs(GetChild(0), typeof(FirearmMagZone))) return;

		FirearmMagZone zone = (FirearmMagZone)GetChild(0);

		if (zone._snappedInteractable != null && zone._snappedInteractable.IsGrabbed())
		{

			Node3D parent = (Node3D)Target.GetParent();
			Vector3 grabPos = zone._snappedInteractable.GetPrimaryInteractor().Controller.GlobalPosition;
			Vector3 locGrab = parent.ToLocal(grabPos);
			Transform3D newXform = Transform;

			float distGrab = grabPos.DistanceTo(StartXform.Origin);
			newXform.Origin = locGrab;
			newXform.Origin = newXform.Origin.Clamp(GetMinOrigin(), GetMaxOrigin());

			t += (float)delta; 
			
			StartToEnd(MiddlePositionRatio(locGrab)); 
		}


		float distEnd = Transform.Origin.DistanceTo(EndXform.Origin);
		float distStart = Transform.Origin.DistanceTo(StartXform.Origin);

		if (distEnd <= 0.01)
		{
			zone.CanUnsnap = true;

			if (_unsnapQueued)
			{
				_unsnapQueued = false;
				zone.Unsnap();
			}
		}
		else
		{
			zone.CanUnsnap = false;
			if (!_unsnapQueued) _unsnapQueued = true;
		}

		if (distStart < 0.001)
		{
			if (zone.CurrentMag != null) zone.CurrentMag.CanChamber = true;
		}
		else
		{
			if (zone.CurrentMag != null) zone.CurrentMag.CanChamber = false;
		}

		if (zone.CurrentMag == null) { 
			Transform =EndXform; 
		}
		
	}

	private async void Exit(InteractableSnapZone zone)
	{
		Tween tween = GetTree().CreateTween();

		tween.TweenProperty(this, "transform", EndXform, 0.2f);

		await ToSignal(tween, "finished");

		zone.Unsnap(true);
	}
}
