using Godot;
using Godot.Collections;
using NXR;
using NXRInteractable;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;



[GlobalClass]
public partial class InteractableSnapZone : Area3D
{


	[Export] private SnapMode _snapMode = SnapMode.OnEnter;
	[Export] public bool Locked = false;
	[Export] public bool DropOnSnap = false; 

	/// <summary>
	/// Groups allowed by this snap zone
	/// Any interactable allowed when left empty 
	/// </summary>
	[Export] public String[] AllowedGroups;
	[Export] private float _snapWaitTime = 0.25f; 

	[Export] private float _tweenTime = 0.1f;
	[Export] private Tween.EaseType _easeType;
	[Export] private Tween.TransitionType _transType;


	#region  DistanceSettings
	[ExportGroup("Distance Settings")]
	[Export] private bool _requireDrop = false; 
	[Export] private float _snapDistance = 0.08f;
	[Export] private float _breakDistance = 0.1f;
	#endregion


	public List<Interactable> HoveredInteractables = new(); 
	public Interactable LastSnappedInteractable { get; set; } = null;
	public Interactable SnappedInteractable { get; set; } = null;
	public Interactable HoveredInteractable { get; set; }

	public bool CanSnap = true;
	public bool CanUnsnap = true;

	private Tween _snapTween;
	

	[Signal] public delegate void OnSnapEventHandler(Interactable interactable);
	[Signal] public delegate void OnUnSnapEventHandler();


	private RemoteTransform3D _rt = new RemoteTransform3D();


	public override void _Ready()
	{
		AddChild(_rt);

		BodyEntered += Entered;
		BodyExited += Exited;

		foreach (Node3D child in GetChildren())
		{
			if (Util.NodeIs(child, typeof(Interactable)))
			{
				HoveredInteractable = (Interactable)child;
				Connect(HoveredInteractable); 
				Snap(HoveredInteractable);

				return;
			}
		}
	}


	public override void _Process(double delta)
	{    

		if (!IsInstanceValid(SnappedInteractable)) { 
			SnappedInteractable = null; 
		} 

		if (_snapMode == SnapMode.Distance)
		{
			if (SnappedInteractable == null && HoveredInteractable != null)
			{
				DistanceSnap(HoveredInteractable);
			}

			if (SnappedInteractable != null)
			{
				if (!_snapTween.IsRunning()) SnappedInteractable.GlobalTransform = GlobalTransform;
				DistanceBreak();
			}
		}

		if (IsInstanceValid(LastSnappedInteractable) && LastSnappedInteractable != null) { 
			if (GetOverlappingBodies().Contains(LastSnappedInteractable)) {
				Connect(LastSnappedInteractable); 
			} 
		}

		if(!IsInstanceValid(SnappedInteractable) && SnappedInteractable != null) { 
			Unsnap(); 
		}

		if (HoveredInteractable != null && !IsInstanceValid(HoveredInteractable)) { 
			HoveredInteractable = null; 
		}
	}


	public virtual async void Unsnap(bool force = true, bool disconnect=true)
	{
		if (Locked) return;

		_snapTween?.Kill(); 
		
		if (IsInstanceValid(SnappedInteractable))
		{
			SnappedInteractable.Reparent(SnappedInteractable.PreviousParent, true);
		}



		_rt.RemotePath = "";
		SnappedInteractable.Freeze = false;
		LastSnappedInteractable = SnappedInteractable; 
		SnappedInteractable = null;
		

	
		EmitSignal("OnUnSnap");

		CanSnap = false;
		await ToSignal(GetTree().CreateTimer(_snapWaitTime), "timeout");

		if (LastSnappedInteractable.HasMeta("Snapped")) { 
			LastSnappedInteractable.RemoveMeta("Snapped"); 
		}

		CanSnap = true;
	}

	
	public virtual async void Snap(Interactable interactable)
	{
		if (!interactable.IsInsideTree()) return;
		if (_snapTween != null && _snapTween.IsRunning()) return; 


		interactable.SetMeta("Snapped", true); 
			
		LastSnappedInteractable = interactable; 
		SnappedInteractable = interactable;
		SnappedInteractable.Freeze = true;

		if (interactable.IsInsideTree())
			SnappedInteractable.Reparent(this, true);
		


		SnapTween();

		if (DropOnSnap)
			SnappedInteractable.FullDrop(); 

		if (IsInstanceValid(_snapTween))
			await ToSignal(_snapTween, "finished");


		Connect(SnappedInteractable);


		EmitSignal("OnSnap", interactable);
	}


	private void SnapTween()
	{
		_snapTween = GetTree().CreateTween();
		_snapTween.SetProcessMode(Tween.TweenProcessMode.Physics); 
		_snapTween.SetParallel(true);
		_snapTween.SetEase(_easeType);
		_snapTween.SetTrans(_transType);
		_snapTween.TweenProperty(SnappedInteractable, "position", Vector3.Zero, _tweenTime);
		_snapTween.TweenProperty(SnappedInteractable, "rotation", Vector3.Zero, _tweenTime);
	}


	private void Entered(Node3D body)
	{      
		if (!CanSnap) return;

		Interactable interactable = Util.NodeIs(body, typeof(Interactable)) ? (Interactable)body : null;
		if (interactable == null || !IsValidInteractable(interactable)) return; 
		

		if (IsInstanceValid(interactable) && interactable != HoveredInteractable) { 
			Disconnect((Interactable)HoveredInteractable); 
		}	

		
		if (HoveredInteractable != null && interactable != HoveredInteractable) { 
			Disconnect(HoveredInteractable); 
			
		}

		HoveredInteractable = interactable;
		Connect(HoveredInteractable);

		if (_snapMode == SnapMode.OnEnter)
		{
			HoveredInteractable.FullDrop();
			Snap(HoveredInteractable);
		}
	}
	

	private void Exited(Node3D body)
	{
		if (!CanSnap) return; 
		if (body == HoveredInteractable && body != SnappedInteractable) { 
			Disconnect(HoveredInteractable); 
		}
	}


	protected void DistanceBreak()
	{

		if (SnappedInteractable == null) return;


		if (!SnappedInteractable.IsGrabbed()) return;

		Interactor interactor = null;
		if (SnappedInteractable.GetPrimaryInteractor() != null)
		{
			interactor = SnappedInteractable.GetPrimaryInteractor();
		}
		else if (SnappedInteractable.GetSecondaryInteractor() != null)
		{
			interactor = SnappedInteractable.GetSecondaryInteractor();
		}

		float distance = interactor.GlobalPosition.DistanceTo(SnappedInteractable.GlobalPosition);
		if (distance > _breakDistance)
		{
			Unsnap();
			return;
		}
	}


	protected void DistanceSnap(Interactable interactable, bool requireDrop=false)
	{

		if (!CanSnap) return;

		if (!IsValidInteractable(interactable)) return; 

		Interactor interactor = null;
		if (interactable.GetPrimaryInteractor() != null)
		{
			interactor = interactable.GetPrimaryInteractor();
		}
		else if (interactable.GetSecondaryInteractor() != null)
		{
			interactor = interactable.GetSecondaryInteractor();
		}

		float distance = interactor.GlobalPosition.DistanceTo(GlobalPosition);
		if (distance < _snapDistance)
		{
			Snap(interactable);
		}
	}


	private void OnDropped(Interactable interactable, Interactor interactor)
	{
		if (_snapMode == SnapMode.OnDrop)
		{
			if (interactor == interactable.PrimaryInteractor && interactable.SecondaryInteractor != null) return; 
			if (interactor == interactable.SecondaryInteractor && interactable.PrimaryInteractor != null) return; 

			CallDeferred("Snap", interactable);
		}
	}


	private void OnGrabbed(Interactable interactable, Interactor interactor)
	{
		if (IsInstanceValid(_snapTween) && _snapTween.IsRunning() && _snapMode != SnapMode.Distance)
		{
			_snapTween.Stop();
		}


		switch (_snapMode)
		{
			case SnapMode.OnEnter:
				Unsnap();
				break;

			case SnapMode.OnDrop: 
				Unsnap(); 
				break;
		}
	}


	private bool InGroup(Node3D interactable)
	{
		bool allowed = AllowedGroups == null || AllowedGroups.Any(group => interactable.GetGroups().Contains(group));

		return allowed;
	}


	private bool IsValidInteractable(Interactable interactable) { 

		if (!IsInstanceValid(interactable) || interactable.HasMeta("Snapped")) return false; 

		return 
			InGroup(interactable) && 
			interactable.IsGrabbed() &&
			SnappedInteractable == null; 
	}


	private List<Interactable> GetOverlappingInteractables() { 
		List<Node3D> bodies = GetOverlappingBodies().ToList(); 

		bodies.RemoveAll(body => !Util.NodeIs(body, typeof(Interactable))); 

		HoveredInteractables.RemoveAll(
			x => 
			!x.IsGrabbed() || 
			!InGroup(x)
			); 

		return HoveredInteractables;  
	}


	private void Connect(Interactable interactable)
	{
		Action<Interactable, Interactor> dropAction = OnDropped;
		Action<Interactable, Interactor> grabAction = OnGrabbed;
		bool dropConnected = interactable.IsConnected("OnDropped", Callable.From(dropAction));
		bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

		if (!dropConnected)
		{
			interactable.Connect("OnDropped", Callable.From(dropAction));
		}
		if (!grabConnected)
		{
			interactable.Connect("OnGrabbed", Callable.From(grabAction));
		}
	}
	

	private void Disconnect(Interactable interactable)
	{
		if (interactable == null) return;

		Action<Interactable, Interactor> dropAction = OnDropped;
		Action<Interactable, Interactor> grabAction = OnGrabbed;
		bool dropConnected = interactable.IsConnected("OnDropped", Callable.From(dropAction));
		bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

		if (dropConnected)
		{
			interactable.Disconnect("OnDropped", Callable.From(dropAction));
		}
		if (grabConnected)
		{

			interactable.Disconnect("OnGrabbed", Callable.From(grabAction));
		}
	}
}