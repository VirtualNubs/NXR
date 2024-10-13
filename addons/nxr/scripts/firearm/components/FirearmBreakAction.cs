using System;
using Godot;
using NXR;

namespace NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmBreakAction : FirearmClampedXform
{
	[Export] private Node3D _bulletQueue;


	public override void _Process(double delta)
	{

		if (Engine.IsEditorHint())
		{
			RunTool();
		}

		if (Firearm == null) return;


		if (Firearm.GetPrimaryInteractor() != null && Firearm.GetPrimaryInteractor().Controller.ButtonOneShot("ax_button"))
		{
			Open();
		}

		if (!IsClosed() && GetCloseInput())
		{
			Close();
		}

		if (!IsClosed() && Firearm.GetSecondaryInteractor() != null)
		{
			Vector3 dir = Firearm.GetSecondaryInteractor().GlobalPosition - Target.GlobalPosition;
			float angle = Target.GlobalTransform.Basis.Y.Dot(dir.Normalized());
			angle = Mathf.Clamp(angle * 1.5f, -1, 1);

			Target.Transform = EndXform.InterpolateWith(StartXform, angle);
		}


		if (!IsClosed())
		{
			
			Firearm.BlockFire = true;
		}
		else
		{
			Firearm.BlockFire = false;
		}
	}


	private bool IsClosed()
	{
		return Target.Transform.IsEqualApprox(StartXform);
	}


	public async void Open()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Target, "transform", EndXform.Orthonormalized(), 0.1f);

		await ToSignal(tween, "finished");

		if (Util.NodeIs(_bulletQueue, typeof(FirearmBulletZoneQueue)))
		{
			FirearmBulletZoneQueue queue = (FirearmBulletZoneQueue)_bulletQueue;

			queue.EjectAll(Target.GlobalTransform.Basis.Z, Vector3.Right, true); 
		}
	}

	public void Close()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Target, "transform", StartXform.Orthonormalized(), 0.1f);
	}

	private bool GetCloseInput()
	{
		if (Firearm.GetPrimaryInteractor() == null) return false;
		Controller controller = Firearm.GetPrimaryInteractor().Controller;
		Vector3 dir = Firearm.GetPrimaryInteractor().Controller.Transform.Basis.Y; 
				
		return controller.VelMatches(dir, 2f);
	}
}
