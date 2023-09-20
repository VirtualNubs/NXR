using Godot;

namespace NXR;

[GlobalClass]
public partial class Interactable : RigidBody3D
{

	[Export]
	public HoldMode HoldMode = NXR.HoldMode.Hold;

	[ExportGroup("Actions")]
	[Export]
	public string GrabAction = "grip_click";

	[ExportGroup("Settings")]
	[Export]
	public float MaxGrabDistance = 0.5f;

	[ExportGroup("Smoothing")]
	[Export]
	public float PositionSmoothing = 0.1f;
	[Export]
	public float RotationSmoothing = 0.1f;


	[ExportGroup("DropBehaviour")]
	[Export]
	private bool _switchOnDrop = false;


	[ExportGroup("Offsets")]
	[Export]
	public Vector3 PositionOffset = new Vector3();

	[Export]
	public Vector3 RotationOffset = new Vector3();

	
	[ExportGroup("Haptics")]
	[Export]
	private float _grabPulse = 0.2f; 

	[Export]
	private float _dropPulse = 0.2f; 


	public Interactor PrimaryInteractor { set; get; }
	public Interactor SecondaryInteractor { set; get; }


	private Transform3D _secondaryRelativeTransorm = new Transform3D();


	[Signal]
	public delegate void OnGrabbedEventHandler(Interactor interactor);

	[Signal]
	public delegate void OnDroppedEventHandler(Interactor interactor);


	public override void _PhysicsProcess(double delta)
	{
		if (IsInstanceValid(PrimaryInteractor))
		{
			Transform3D xform = GlobalTransform;
			Transform3D primaryXform = PrimaryInteractor.GlobalTransform;
			Vector3 rotOffset = RotationOffset * (Vector3.One * (Mathf.Pi / 180));

			xform.Origin = GlobalTransform.Origin.Lerp(primaryXform.Origin, 1.0f);
			xform.Basis = primaryXform.Basis * Basis.FromEuler(rotOffset);

			GlobalTransform = xform;
			CallDeferred("SetOffsets", xform.TranslatedLocal(PositionOffset)); 
		}

		if (IsInstanceValid(SecondaryInteractor) && !IsInstanceValid(PrimaryInteractor))
		{
			// TODO: handle behaviour when only secondary interactor available 
		}
	}

	public void Grab(Interactor interactor)
	{
		if (!IsInstanceValid(PrimaryInteractor))
		{
			PrimaryInteractor = interactor;
		}
		else
		{
			SecondaryInteractor = interactor;
		}

		EmitSignal("OnGrabbed", interactor);
	}

	public void Drop(Interactor interactor)
	{


		if (interactor == PrimaryInteractor)
		{
			PrimaryInteractor = null;
		}
		else
		{
			SecondaryInteractor = null;
		}

		if (IsInstanceValid(SecondaryInteractor))
		{

			_secondaryRelativeTransorm = SecondaryInteractor.GlobalTransform.AffineInverse() * GlobalTransform;


			// switch primary grab if secondary grabber found 
			if (_switchOnDrop)
			{
				SecondaryInteractor.Grab(this);
				SecondaryInteractor = null;
			}
		}

		EmitSignal("OnDropped", interactor);
	}

	private void SetOffsets(Transform3D xform)
	{
		GlobalTransform = xform; 
	}

	public bool IsGrabbed()
	{
		return IsInstanceValid(PrimaryInteractor) || IsInstanceValid(SecondaryInteractor);
	}

	public bool IsInteractable()
	{
		return true;
	}
}
