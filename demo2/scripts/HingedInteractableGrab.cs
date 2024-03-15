using System;
using Godot;
using NXRInteractable;

/// <summary>
/// A type of InteractableGrab that hinges off an object. The Interactable 
/// this is a part of will rotate and translate as if it is hinged like a
/// door. The Interactable will always rotate about the hinge's local Y
/// axis, rotate it accordingly. 
/// </summary>
[GlobalClass]
public partial class HingedInteractableGrab : InteractableGrab
{
	[ExportGroup("Hinge Settings")]
	[Export]
	private Node3D Hinge { set; get; }

	private Vector3 _interactableToHinge;
	private Transform3D _interactableInitGlobalTransform;

	public override void _Ready()
	{
		base._Ready();

		Hinge ??= Interactable.GetParent<Node3D>();

		// Not a null check. Checking if the hinge has Transform3D information
#pragma warning disable IDE0150 // Prefer 'null' check over type check
		if (Hinge is not Node3D)
		{
			GD.PrintErr(@$"{Name} as HingedInteractableGrab has no valid Hinge 
			target. {Hinge.Name} is not a Node3D nor derived from Node3D");
			Hinge = null;
		}
#pragma warning restore IDE0150 // Prefer 'null' check over type check

		_interactableToHinge = Interactable.GlobalPosition - Hinge.GlobalPosition;
		_interactableInitGlobalTransform = Interactable.GlobalTransform;
	}

	public override void _PhysicsProcess(double delta)
	{
		KinematicGrab((float)delta);
	}

	protected override void KinematicGrab(float delta)
	{
		// Do not hinge if there is no Hinge
		if (Hinge is null)
		{
			GD.PrintErr($"{Name} has no Hinge.");
			return;
		};

		if (IsInstanceValid(Interactable.PrimaryInteractor))
		{
			Node3D grabPoint = Interactable.PrimaryGrabPoint;
			Interactor interactor = Interactable.PrimaryInteractor;
			Basis rotOffset = (Interactable.GlobalTransform.Basis.Inverse() * grabPoint.GlobalTransform.Basis).Orthonormalized();
			Basis newRot = rotOffset.Orthonormalized();

			_rotationDelta = Mathf.Lerp(_rotationDelta, 1.0f, delta * _rotationSmoothing);
			_positionDelta = Mathf.Lerp(_positionDelta, 1.0f, delta * _positionSmoothing);

			Node3D hinge = Interactable.GetParent<Node3D>();

			// Calculating newPos based on the initial distance between the Interactable and the Hinge
			Vector3 newPos = interactor.GlobalPosition;
			Vector3 actualPos = hinge.GlobalPosition + (_interactableToHinge.Length() * (newPos - hinge.GlobalPosition).Normalized());
			newPos = actualPos;
			newPos.Y = Interactable.GlobalPosition.Y;   // No translation in the Hinge axis

			// Calculating newRot based on the angle between the handle's posision and newPos
			Vector3 hingeToHandle = newPos - hinge.GlobalPosition; // Get vector from hinge to newPos
			float handleAngle = hinge.GlobalBasis.X.SignedAngleTo(hingeToHandle, hinge.GlobalTransform.Basis.Orthonormalized().Y); // Calculate angle from hinge's X 
																																   // basis and previous vector
			newRot = newRot.Rotated(hinge.Basis.Y.Normalized(), handleAngle); // Get rotated newRot based on angle

			_primaryXform.Basis = Interactable.GlobalTransform.Basis.Orthonormalized().Slerp(newRot.Orthonormalized(), _rotationDelta);
			_primaryXform.Origin = Interactable.GlobalTransform.Origin.Slerp(newPos, _positionDelta);

			if (_percise) { _primaryXform = Interactable.GetPrimaryRelativeXform(); }

			if (Math.Abs(Mathf.RadToDeg(handleAngle)) >= 10f)
			{
				Interactable.GlobalTransform = _primaryXform * Interactable.GetOffsetXform();
			}
			else
			{
				Interactable.GlobalTransform = _interactableInitGlobalTransform;
			}
		}
	}
}
