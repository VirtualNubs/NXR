using Godot;

namespace NXR;

[GlobalClass]
public partial class Interactable : RigidBody3D
{
	[Export]
	public InteratableType Type = InteratableType.Kinamatic; 

	[Export]
	public HoldMode HoldMode = NXR.HoldMode.Hold;

	[ExportGroup("Actions")]
	[Export]
	public string GrabAction = "grip_click";

	[ExportGroup("Settings")]
	[Export]
	public float MaxGrabDistance = 0.5f;

	[ExportGroup("Smoothing")]
	[Export(PropertyHint.Range, "0.0, 1")]
	public float PositionSmoothing = 1f;
    [Export(PropertyHint.Range, "0.0, 1")]
    public float RotationSmoothing = 1f;


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


	public Interactor PrimaryInteractor { set; get; }
	public Interactor SecondaryInteractor { set; get; }


	private Transform3D _secondaryRelativeTransorm = new Transform3D();


	public Generic6DofJoint3D PrimaryGrabJoint; 
	public Generic6DofJoint3D SecondaryGrabJoint;

	private bool _initFreezeState = false; 


	[Signal]
	public delegate void OnGrabbedEventHandler(Interactable interactable, Interactor interactor);

	[Signal]
	public delegate void OnDroppedEventHandler(Interactable interactable, Interactor interactor);


    public override void _Ready()
    {
		_initFreezeState = Freeze; 
    }

    public override void _PhysicsProcess(double delta)
	{
		if (Type == InteratableType.Physics)
		{
			return; 
		}

		if (IsInstanceValid(PrimaryInteractor))
		{
			LinearVelocity = Vector3.Zero; 

			Transform3D xform = GlobalTransform;
			Transform3D primaryXform = PrimaryInteractor.GlobalTransform;
			Vector3 rotOffset = RotationOffset * (Vector3.One * (Mathf.Pi / 180));

			xform.Origin = GlobalTransform.Origin.Lerp(primaryXform.Origin, PositionSmoothing);
			xform.Basis = xform.Basis.Slerp(primaryXform.Basis, RotationSmoothing) ;

			Transform3D offsetTransform = (GlobalTransform * GlobalTransform.AffineInverse()); 
			offsetTransform = offsetTransform.TranslatedLocal(PositionOffset);
            offsetTransform.Basis *= Basis.FromEuler(rotOffset);

            GlobalTransform = xform * offsetTransform;
		}

		if (IsInstanceValid(SecondaryInteractor) && !IsInstanceValid(PrimaryInteractor))
		{
			// TODO: handle behaviour when only secondary interactor available 
		}
	}

	public void Grab(Interactor interactor)
	{

		Freeze = true; 

		if (!IsInstanceValid(PrimaryInteractor))
		{
            PrimaryInteractor = interactor;
            PrimaryInteractor.Controller.Pulse(0.5f, _grabPulse, 0.1);
		}

		else
		{
            SecondaryInteractor = interactor;
            SecondaryInteractor.Controller.Pulse(0.5f, _grabPulse, 0.1);
		}

        EmitSignal("OnGrabbed", this, interactor);

	}

	public void Drop(Interactor interactor)
	{

		// emit before so we can access any set interactor before setting null 
		EmitSignal("OnDropped", this, interactor);

		Freeze = _initFreezeState; 

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

	public bool IsGrabbed()
	{
		return IsInstanceValid(PrimaryInteractor) || IsInstanceValid(SecondaryInteractor);
	}

	public bool IsInteractable()
	{
		return true;
	}
}
