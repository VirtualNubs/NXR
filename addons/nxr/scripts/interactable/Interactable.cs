using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Godot;

namespace NXRInteractable;

[GlobalClass]
public partial class Interactable : RigidBody3D
{

	#region Exported: 
	[Export] public bool Disabled { get; set; } = false;
	[Export(PropertyHint.Range, "0.1, 100")] public float Priority { get; set; } = 1;
	[Export] public HoldMode HoldMode { get; set; } = HoldMode.Hold;


	[ExportGroup("Grab Settings")]
	[Export] public bool DistanceGrabEnabled { get; set; } = false;
	[Export] public float DistanceGrabReach { get; set; } = 4;
	[Export] public float GrabBreakDistance { get; set; } = 0.5f;


	[ExportGroup("Action Settings")]
	[Export] public string GrabAction { get; set; } = "grip_click";
	[Export] public string DropAction { get; set; } = "grip_click";


	[ExportGroup("Drop Settings")]
	[Export] private bool _switchOnDrop = false;


	[ExportGroup("Haptic Settings")]
	[Export(PropertyHint.Range, "0.0, 1")] private float _grabPulse = 0.1f;
	[Export(PropertyHint.Range, "0.0, 1")] private float _dropPulse = 0.1f;


	[ExportGroup("Offsets")]
	public Vector3 PositionOffset { get; set; } = new Vector3();
	public Vector3 RotationOffset { get; set; } = new Vector3();
	#endregion


	#region Public: 
	public Node3D PreviousParent { get; set; }
	public Node3D InitParent { get; set; } = null;
	public Transform3D InitTransform { set; get; }
	public bool InitFreeze { get; set; } = false;
	public Transform3D InitGlobalTransform { set; get; }
	public Interactor PrimaryInteractor { set; get; }
	public Interactor SecondaryInteractor { set; get; }

	public delegate void IntegrateForcesDelegate(PhysicsDirectBodyState3D state);
	public IntegrateForcesDelegate IntegrateForces { set; get; }

	public Node3D PrimaryGrabPoint;
	public Node3D SecondaryGrabPoint;
	public Transform3D PrimaryGrabPointOffset = new();
	public Transform3D SecondaryGrabPointOffset = new();
	public bool Hovered = false;
	public Transform3D PrimaryGrabXform = new Transform3D();
	#endregion


	#region Private: 
	protected Transform3D _secondaryRelativeTransorm = new Transform3D();
	protected Transform3D _primaryRelativeTransform = new Transform3D();
	#endregion


	#region Signals: 
	[Signal] public delegate void OnGrabbedEventHandler(Interactable interactable, Interactor interactor);
	[Signal] public delegate void OnDroppedEventHandler(Interactable interactable, Interactor interactor);
	[Signal] public delegate void OnFullDroppedEventHandler();
	[Signal] public delegate void StateUpdatedEventHandler(PhysicsDirectBodyState3D state3D);
	#endregion


	public override void _Ready()
	{
		InitTransform = Transform;
		InitGlobalTransform = Transform;
		InitFreeze = Freeze;
		InitParent = (Node3D)GetParent();
		PreviousParent = InitParent;

		PrimaryGrabPoint ??= this;
		SecondaryGrabPoint ??= this;
	}


	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (IntegrateForces is not null)
		{
			IntegrateForces(state);
		};
	}


	public void Grab(Interactor interactor)
	{
		if (Disabled) return;

		interactor.GrabbedInteractable = this;

		if (!IsInstanceValid(PrimaryInteractor))
		{
			PrimaryInteractor = interactor;
			PrimaryInteractor.Controller.Pulse(0.5f, _grabPulse, 0.1);
			PrimaryGrabXform = interactor.GlobalTransform;
			_primaryRelativeTransform = interactor.GlobalTransform.AffineInverse() * GlobalTransform;
		}
		else
		{
			SecondaryInteractor = interactor;
			SecondaryInteractor.Controller.Pulse(0.5f, _grabPulse, 0.1);
			_secondaryRelativeTransorm = SecondaryInteractor.GlobalTransform.AffineInverse() * GlobalTransform;
		}

		// emit after to access available interactors
		EmitSignal("OnGrabbed", this, interactor);
	}

	public void SecondaryGrab(Interactor interactor)
	{

		if (Disabled) return;

		if (!IsInstanceValid(SecondaryInteractor))
		{
			interactor.GrabbedInteractable = this;
			SecondaryInteractor = interactor;
			SecondaryInteractor.Controller.Pulse(0.5f, _grabPulse, 0.1);
			_secondaryRelativeTransorm = SecondaryInteractor.GlobalTransform.AffineInverse() * GlobalTransform;
		}

		// emit after to access available interactors
		EmitSignal("OnGrabbed", this, interactor);
	}

	public void Drop(Interactor interactor)
	{
		// emit before so we can access any set interactor before setting null 
		EmitSignal("OnDropped", this, interactor);

		if (interactor == PrimaryInteractor)
		{
			PrimaryInteractor.Controller.Pulse(0.5f, _dropPulse, 0.1);
			PrimaryInteractor = null;
			PrimaryGrabXform = interactor.GlobalTransform;
		}

		if (interactor == SecondaryInteractor)
		{
			SecondaryInteractor.Controller.Pulse(0.5f, _dropPulse, 0.1);
			SecondaryInteractor = null;
		}

		if (IsInstanceValid(SecondaryInteractor))
		{
			_secondaryRelativeTransorm = SecondaryInteractor.GlobalTransform.AffineInverse() * GlobalTransform;
			// switch primary grab if secondary grabber found 
			if (_switchOnDrop && interactor != SecondaryInteractor)
			{
				Interactor newPrimary = SecondaryInteractor;
				SecondaryInteractor.Drop();
				newPrimary.Grab(this);
			}
		}

		if (!IsGrabbed())
		{
			Freeze = InitFreeze;
			LinearVelocity = interactor.Controller.GetGlobalVelocity();
			AngularVelocity = interactor.Controller.GetAngularVelocity();
			EmitSignal("OnFullDropped");
		}
	}

	public void FullDrop()
	{
		if (IsInstanceValid(PrimaryInteractor))
		{
			PrimaryInteractor.Drop();
		}
		if (IsInstanceValid(SecondaryInteractor))
		{
			SecondaryInteractor.Drop();
		}
	}

	public Interactor GetPrimaryInteractor()
	{
		if (PrimaryInteractor != null)
		{
			return PrimaryInteractor;
		}
		return null;
	}

	public Interactor GetSecondaryInteractor()
	{
		if (SecondaryInteractor != null)
		{
			return SecondaryInteractor;
		}
		return null;
	}

	public Transform3D GetPrimaryRelativeXform()
	{
		return PrimaryInteractor.GlobalTransform * _primaryRelativeTransform;
	}

	public Transform3D GetSecondaryRelativeXform()
	{
		return SecondaryInteractor.GlobalTransform * _secondaryRelativeTransorm;
	}

	public Transform3D GetOffsetXform()
	{
		Vector3 rotOffset = RotationOffset * (Vector3.One * (Mathf.Pi / 180));
		Transform3D offsetTransform = (GlobalTransform * GlobalTransform.AffineInverse());
		offsetTransform = offsetTransform.TranslatedLocal(PositionOffset);
		offsetTransform.Basis *= Basis.FromEuler(rotOffset);
		return offsetTransform.Orthonormalized();
	}

	public bool IsGrabbed()
	{
		return IsInstanceValid(PrimaryInteractor) || IsInstanceValid(SecondaryInteractor);
	}

	public bool IsTwoHanded()
	{
		return IsInstanceValid(PrimaryInteractor) && IsInstanceValid(SecondaryInteractor);
	}

	public void SetPhysicState(Vector3 linearVelocity, Vector3 angularVelocity, PhysicsDirectBodyState3D state)
	{
		state.LinearVelocity = linearVelocity;
		state.AngularVelocity = angularVelocity;
	}

	public bool IsInteractable()
	{
		return true;
	}
}