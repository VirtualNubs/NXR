using System;
using System.Data.Common;
using Godot;
using NXR;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmMagZoneTrack : FirearmClampedXform
{
	[Export] private float pullStrength = 2.0f;

	private bool _unsnapQueued = false;
	private FirearmMagZone _zone;
	private float t = 0.0f;


	public override void _Ready()
	{
		if (!Util.NodeIs(GetChild(0), typeof(FirearmMagZone))) return;

		_zone = (FirearmMagZone)GetChild(0);
		_zone.TryEject += TryEject;
		_zone.EjectEnabled = false;

		Transform = StartXform;

		Freeze = true;
	}


	public override void _Process(double delta)
	{
		RunTool();


		if (_zone == null) return;

		if (_zone.SnappedInteractable != null && _zone.SnappedInteractable.IsGrabbed())
		{

			Node3D parent = (Node3D)Target.GetParent();
			Vector3 grabPos = _zone.SnappedInteractable.GetPrimaryInteractor().Controller.GlobalPosition;
			Vector3 locGrab = parent.ToLocal(grabPos);
			Transform3D newXform = Transform;

			newXform.Origin = locGrab;
			newXform.Origin = newXform.Origin.Clamp(GetMinOrigin(), GetMaxOrigin());
			t += (float)delta;

			StartToEnd(MiddlePositionRatio(locGrab));
		}


		float distEnd = Transform.Origin.DistanceTo(EndXform.Origin);
		float distStart = Transform.Origin.DistanceTo(StartXform.Origin);

		if (!AtEnd())
		{
			_zone.CanUnsnap = false;
		}
		else
		{
			_zone.CanUnsnap = true;
		}

		if (distStart < 0.001)
		{
			if (_zone.CurrentMag != null) _zone.CurrentMag.CanChamber = true;
		}
		else
		{
			if (_zone.CurrentMag != null) _zone.CurrentMag.CanChamber = false;
		}

		if (_zone.CurrentMag == null)
		{
			Transform = EndXform;
		}
		if (AtStart() && _zone.SnappedInteractable.IsGrabbed())
		{
			_zone.SnappedInteractable.FullDrop();
		}
	}


	private async void TryEject()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "transform", EndXform, 0.1f);

		await ToSignal(tween, "finished");

		if (_zone.GetFirearm() != null && _zone.GetFirearm().PrimaryInteractor != null)
		{
			Vector3 anguler = _zone.CurrentMag.AngularVelocity = _zone.GetFirearm().PrimaryInteractor.Controller.GetAngularVelocity();
			float angLength = anguler.LimitLength(3).Length();

			_zone.CurrentMag.LinearVelocity = _zone.GetFirearm().PrimaryInteractor.Controller.GetGlobalVelocity() * (angLength);
			_zone.CurrentMag.AngularVelocity = anguler;
		}
		_zone.Unsnap();
	}


	private async void Exit(InteractableSnapZone zone)
	{
		Tween tween = GetTree().CreateTween();

		tween.TweenProperty(this, "transform", EndXform, 0.2f);

		await ToSignal(tween, "finished");

		zone.Unsnap();
	}
}
