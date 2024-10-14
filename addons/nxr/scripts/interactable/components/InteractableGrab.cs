using Godot;
using Godot.NativeInterop;
using NXR;
using NXRInteractable;

[GlobalClass]
public partial class InteractableGrab : Node
{

	#region Exported
	[Export] private GrabType _grabType = GrabType.Static;
	[Export] private bool _percise = false;
	[Export] private bool _parentToInteractor = false; 


	[ExportGroup("SecondaryGrabBehavior")]
	[Export] private bool _twoHanded = false;
	[Export] private LookUpVector _lookUpVector = LookUpVector.PrimaryInteractor;
	[Export] private bool invert = false;


	[ExportGroup("Physics Grab Settings")]
	private float _initLinearDamp = 0.0f;
	private float _initAngularDamp = 0.0f;
	#endregion


	[ExportGroup("Ease Settings")]
	[Export] private float _rotationEaseTime = 0.2f;
	[Export] private float _positionEaseTime = 0.2f;



	#region Public     
	public Interactable Interactable { get; set; }
	#endregion


	#region Private  
	private Vector3 _perciseOffset = new();
	private Vector3 lVelocity = Vector3.Zero;
	private Vector3 aVelocity = Vector3.Zero;
	private bool _initFreezeState = false;
	private float _positionEase = 0.0f;
	private float _rotationEase = 0.0f;
	private float _secondaryRotationEase = 0.0f;
	private Tween _grabTween;
	private Tween _secondaryRotTween;
	private Tween _posTween;
	#endregion



	public override void _Ready()
	{
		if (Util.GetNodeFromParentOrOwnerType(this, typeof(Interactable)) != null)
		{
			Interactable = (Interactable)GetParent();
			Interactable.OnGrabbed += OnGrab;
			Interactable.OnFullDropped += OnFullDrop;
			Interactable.OnDropped += OnDrop;
			Interactable.OnDropped += CleanupEase;
			_initFreezeState = Interactable.Freeze;
			_initLinearDamp = Interactable.LinearDamp;
			_initAngularDamp = Interactable.AngularDamp;
		}
	}


    public override void _PhysicsProcess(double delta)
	{
		switch (_grabType) { 
			case GrabType.Static: 
				KinematicGrab((float)delta); 
				break; 
			case GrabType.Physics: 
				PhysicsGrab(delta); 
				break; 
		}
	}


	public void OnGrab(Interactable interactable, Interactor interactor)
	{
		HandleEase(interactor);

		if (interactor == Interactable.PrimaryInteractor)
		{
			Interactable.Freeze = _grabType == GrabType.Static;
			Interactable.LinearVelocity = Vector3.Zero;


			if (_parentToInteractor) { 
				Interactable.Reparent(interactor, true); 
				Interactable.PreviousParent = (Node3D)interactor; 
			}
		}
	}


	public void OnDrop(Interactable interactable, Interactor interactor)
	{
		_perciseOffset = Vector3.Zero;
	}
	

	public void OnFullDrop() { 
		if (_parentToInteractor && Util.NodeIs(Interactable.GetParent(), typeof(Interactor))) {
			Interactable.PreviousParent = Interactable.InitParent; 
			Interactable.Reparent(Interactable.InitParent, true); 
		}
	}


	private void PhysicsGrab(double delta)
	{
		if (Interactable.GetPrimaryInteractor() != null)
		{

			Vector3 linearPredict = Interactable.GetPrimaryInteractor().Controller.GetGlobalVelocity() * (float)delta;
			Quaternion currentRotation = Interactable.GlobalTransform.Basis.GetRotationQuaternion();
			Quaternion previousRotation = GetGrabXform().Basis.GetRotationQuaternion();
			Quaternion rotationChange = currentRotation * previousRotation.Inverse();
			float gravity = 9.8f; 
	
			Vector3 prevVel = Vector3.Zero; 


			lVelocity = (GetGrabXform().Origin - Interactable.GlobalPosition);
			aVelocity = rotationChange.Inverse().GetEuler();

			if (_percise)
			{
				lVelocity = (Interactable.GetPrimaryRelativeXform().Origin - Interactable.GlobalPosition) ;
			}


			if (Interactable.IsTwoHanded())
			{
				Quaternion current = GetTwoHandXform().Basis.GetRotationQuaternion();
				Quaternion prev = Interactable.GlobalTransform.Basis.GetRotationQuaternion();
				Quaternion change = current * prev.Inverse();
				aVelocity = change.GetEuler();
			}
			

			lVelocity /= (float)delta * Interactable.Mass;
			aVelocity /= (float)delta * Interactable.Mass;

			Interactable.LinearVelocity = lVelocity;
			Interactable.AngularVelocity = aVelocity;
		}
	}


	private void KinematicGrab(float delta)
	{
		if (IsInstanceValid(Interactable.PrimaryInteractor))
		{
			Transform3D grabXform = GetGrabXform();

			if (_percise)  grabXform = Interactable.GetPrimaryRelativeXform(); 

			if (_twoHanded && Interactable.IsTwoHanded())
			{
				Interactable.GlobalTransform = GetTwoHandXform().Orthonormalized();
			}
			else
			{
				Interactable.GlobalTransform = grabXform;
			}

			Interactable.GlobalTransform = Interactable.GlobalTransform.TranslatedLocal(Interactable.GetOffsetXform().Origin);
		}

		if (IsInstanceValid(Interactable.SecondaryInteractor) && !IsInstanceValid(Interactable.PrimaryInteractor))
		{
			Interactable.GlobalTransform = Interactable.GetSecondaryRelativeXform();
		}
	}


	private void HandleEase(Interactor interactor)
	{
		_grabTween?.Kill();

		if (interactor == Interactable.PrimaryInteractor)
		{
			_grabTween = GetTree().CreateTween();
			_grabTween.SetParallel(true); 
			_rotationEase = 0.0f;
			_grabTween.TweenProperty(this, "_rotationEase", 1.0, _rotationEaseTime);
			_grabTween.TweenProperty(this, "_positionEase", 1.0, _positionEaseTime);

		}
		else
		{
			_grabTween.SetParallel(true); 
			_grabTween = GetTree().CreateTween();
			_secondaryRotationEase = 0.0f;
			_grabTween.TweenProperty(this, "_secondaryRotationEase", 1, _rotationEaseTime);
		}
	}


	private void CleanupEase(Interactable interactable, Interactor interactor)
	{
		_grabTween?.Kill();

		if (interactor == Interactable.SecondaryInteractor)
		{
			_grabTween.SetParallel(true); 
			_grabTween = GetTree().CreateTween();
			_rotationEase = 0.0f;
			_grabTween.TweenProperty(this, "_rotationEase", 1.0, _rotationEaseTime);
		}

		 if (interactor == Interactable.PrimaryInteractor)
		{
			_positionEase = 0.0f; 
		}
	}


	private Transform3D GetGrabXform()
	{
		Transform3D xform = new();
		Node3D grabPoint = Interactable.PrimaryGrabPoint;
		Interactor interactor = Interactable.PrimaryInteractor;
		Vector3 posOffset = Interactable.GlobalPosition - Interactable.PrimaryGrabPoint.GlobalPosition;
		Vector3 newPos = interactor.GlobalPosition + posOffset;
		Basis rotOffset = (Interactable.GlobalTransform.Basis.Inverse() * grabPoint.GlobalTransform.Basis).Orthonormalized();
		
		

		xform.Origin = Interactable.GlobalTransform.Origin.Lerp(newPos, _positionEase);

		xform.Basis = Interactable.GlobalTransform.Basis.Orthonormalized().Slerp(
			(interactor.GlobalTransform.Basis * rotOffset.Inverse()) * Interactable.GetOffsetXform().Basis,
			_rotationEase
		).Orthonormalized();


		return xform;
	}


	public Transform3D GetTwoHandXform()
	{
		Transform3D grabPointXform = new Transform3D(
			Interactable.GlobalTransform.Basis,
			Interactable.SecondaryGrabPoint.GlobalTransform.Origin
		);
		
		Transform3D secondaryXform = Interactable.SecondaryInteractor.GlobalTransform;


		Transform3D interactableXform = Interactable.GlobalTransform;
		Transform3D lookXform = GetGrabXform();
		Vector3 up = Interactable.GlobalTransform.Basis.Y + GetUpVector();

	
		Vector3 controllerOffset = secondaryXform.Origin - GetGrabXform().Origin;
		Vector3 offset = (grabPointXform.Origin - interactableXform.Origin);
		float dot = controllerOffset.Normalized().Dot(Interactable.GlobalBasis.Z); 
		offset = offset.Reflect(interactableXform.Basis.Z.Normalized()) * Mathf.Abs(dot);
		
		
		Vector3 lookDir = offset + secondaryXform.Origin - GetGrabXform().Origin;
		
		lookXform.Origin = GetGrabXform().Origin; 
		lookXform.Basis = interactableXform.Basis.Slerp(Basis.LookingAt(
			lookDir.Normalized(),
			up.Normalized()).Orthonormalized() * Interactable.GetOffsetXform().Basis,
			_secondaryRotationEase
		);

 
		return lookXform;
	}

   
	public Vector3 GetUpVector()
	{
		switch (_lookUpVector)
		{
			case LookUpVector.PrimaryInteractor:
				return Interactable.PrimaryInteractor.GlobalTransform.Basis.Y;
			case LookUpVector.SecondaryInteractor:
				return Interactable.SecondaryInteractor.GlobalTransform.Basis.Y;
			case LookUpVector.Combined:
				return (Interactable.PrimaryInteractor.GlobalTransform.Basis.Y + Interactable.SecondaryInteractor.GlobalTransform.Basis.Y).Normalized();
		}
		
		return Vector3.Up;
	}
}


