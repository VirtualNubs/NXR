using System;
using Godot;
using NXR;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmBreakAction : FirearmMovable
{

	[Export]
	private Firearm _firearm;

	[Export]
	private Node3D _bulletQueue;

	public override void _Ready()
	{

		if (Util.NodeIs((Node)GetParent(), typeof(Firearm)))
		{
			_firearm = (Firearm)GetParent();
		}
	}

	public override void _Process(double delta)
	{

		if (Engine.IsEditorHint())
		{
			RunTool();
		}

		if (_firearm == null) return;


		if (_firearm.GetPrimaryInteractor() != null && _firearm.GetPrimaryInteractor().Controller.ButtonOneShot("ax_button"))
		{
			Open();

		}

		if (!IsClosed() && GetCloseInput())
		{
			Close();
		}

		if (!IsClosed() && _firearm.GetSecondaryInteractor() != null)
		{
			Vector3 dir = _firearm.GetSecondaryInteractor().GlobalPosition - Target.GlobalPosition;
			float angle = -Target.GlobalTransform.Basis.Y.SignedAngleTo(dir.Normalized(), Vector3.Forward);
			GD.Print(angle);
			angle = Mathf.Clamp(angle, -1, 1);
			Target.Transform = EndXform.InterpolateWith(StartXform, angle);
		}


		if (!IsClosed())
		{
			_firearm.BlockFire = true;
		}
		else
		{
			_firearm.BlockFire = false;
		}
	}

	private bool IsClosed()
	{
		return Target.Transform.IsEqualApprox(StartXform);
	}


	public async void Open()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Target, "transform", EndXform, 0.1f);

		await ToSignal(tween, "finished");

		if (Util.NodeIs(_bulletQueue, typeof(FirearmBulletZoneQueue)))
		{
			FirearmBulletZoneQueue queue = (FirearmBulletZoneQueue)_bulletQueue;

			queue.EjectAll(Target.GlobalTransform.Basis.Z * 2, Vector3.Right * 100000, true); 
		}
	}

	public void Close()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Target, "transform", StartXform, 0.1f);
	}

	private bool GetCloseInput()
	{
		if (_firearm.GetPrimaryInteractor() == null) return false;

		Controller controller = _firearm.GetPrimaryInteractor().Controller;

		return controller.LocalVelMatches(GlobalTransform.Basis.Y, 30);
	}
}
