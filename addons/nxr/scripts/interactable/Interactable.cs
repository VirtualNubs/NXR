using System.Collections.Generic;
using Godot;

namespace NXRInteractable;

[GlobalClass]
public partial class Interactable : RigidBody3D
{
	[Export]
	public float Priority = 1; 
	
	[Export]
	public float MaxGrabDistance = 0.5f;
	

	[Export]
	public HoldMode HoldMode = HoldMode.Hold;

	[ExportGroup("Actions")]
	[Export]
	public string GrabAction = "grip_click";


	
	[ExportGroup("DropBehaviour")]
	[Export]
	private bool _switchOnDrop = false;

	[ExportGroup("Offsets")]
	[Export]
	public Vector3 PositionOffset = new Vector3();
	[Export]
	public Vector3 RotationOffset = new Vector3();


	[ExportGroup("Haptics")]
	[Export(PropertyHint.Range, "0.0, 1")]
	private float _grabPulse = 0.1f;

	[Export(PropertyHint.Range, "0.0, 1")]
	private float _dropPulse = 0.1f;


	public bool InitFreeze = false; 
	public Transform3D InitTransform {set; get; } 
	public Transform3D InitGlobalTransform {set; get; } 
	public Interactor PrimaryInteractor { set; get; }
	public Interactor SecondaryInteractor { set; get; }

	// grab point
	public Node3D PrimaryGrabPoint;
	public Node3D SecondaryGrabPoint;	
	public Transform3D PrimaryGrabPointOffset = new(); 
	public Transform3D SecondaryGrabPointOffset = new(); 

	public Node3D InitParent = null; 

	private Transform3D _secondaryRelativeTransorm = new Transform3D();
	private Transform3D _primaryRelativeTransform = new Transform3D();
	public Transform3D _primaryGrabTransorm = new Transform3D(); 


	[Signal]
	public delegate void OnGrabbedEventHandler(Interactable interactable, Interactor interactor);

	[Signal]
	public delegate void OnDroppedEventHandler(Interactable interactable, Interactor interactor);

	[Signal]
	public delegate void OnFullDroppedEventHandler();


    public override void _Ready()
    {
		PrimaryGrabPoint  ??= this;
        SecondaryGrabPoint  ??= this; 
		InitParent = (Node3D)GetParent(); 
		InitTransform = Transform; 
		InitFreeze = Freeze; 
		InitGlobalTransform = Transform; 
    }

	public void Grab(Interactor interactor)
	{

		interactor._grabbedInteractable = this; 

		if (!IsInstanceValid(PrimaryInteractor))
		{
			PrimaryInteractor = interactor;
			PrimaryInteractor.Controller.Pulse(0.5f, _grabPulse, 0.1);
			_primaryRelativeTransform = interactor.GlobalTransform.AffineInverse() * GlobalTransform;
			_primaryGrabTransorm = interactor.GlobalTransform; 
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


		if (!IsInstanceValid(SecondaryInteractor))
		{
			interactor._grabbedInteractable = this; 
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

		
		if (!IsGrabbed()) { 
			EmitSignal("OnFullDropped"); 
			LinearVelocity = interactor.Controller.GetGlobalVelocity();
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

	public Interactor GetPrimaryInteractor() { 
		if (PrimaryInteractor != null) { 
			return PrimaryInteractor; 
		}
		return null; 
	}

	public Interactor GetSecondaryInteractor() { 
		if (SecondaryInteractor != null) { 
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
		offsetTransform.Basis *= Basis.FromEuler(rotOffset);
		offsetTransform = offsetTransform.TranslatedLocal(PositionOffset);
		return offsetTransform;
	}

	public bool IsGrabbed()
	{
		return IsInstanceValid(PrimaryInteractor) || IsInstanceValid(SecondaryInteractor);
	}

	public bool IsTwoHanded() { 
		return IsInstanceValid(PrimaryInteractor) && IsInstanceValid(SecondaryInteractor); 
	}

	public bool IsInteractable()
	{
		return true;
	}
}
