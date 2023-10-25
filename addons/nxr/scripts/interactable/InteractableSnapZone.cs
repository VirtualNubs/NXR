using Godot;
using Godot.Collections;
using NXR;
using NXRInteractable;
using System;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class InteractableSnapZone : Area3D
{
    [Export]
    public bool Locked = false;

    [Export]
    public String[] AllowedGroups;

    [Export]
    private SnapMode _snapMode = SnapMode.OnEnter;

    [Export]
    private bool _sticky = false;

    [ExportGroup("Tween Settings")]
    [Export]
    private float _tweenTime = 0.1f;
    [Export]
    private Tween.EaseType _easeType;
    [Export]
    private Tween.TransitionType _transType;


    [ExportGroup("Sticky Settings")]
    [Export]
    private float _breakDistance = 0.3f;

    public Interactable _snappedInteractable = null;
    public Interactable _hoveredInteractable;

    public bool CanUnsnap = true; 
    private Vector3 _snappedInitScale;
    private Interactor _lastInteractor;

    private Node3D _snappedInitParent;

    private Tween tween;

    [Signal]
    public delegate void OnSnapEventHandler(Interactable interactable);
    [Signal]
    public delegate void OnUnSnapEventHandler();

    public override void _Ready()
    {
        AreaEntered += Entered;
        AreaExited += Exited;

        foreach (Node3D child in GetChildren())
        {
            if (Util.NodeIs(child, typeof(Interactable)))
            {
                _hoveredInteractable = (Interactable)child;
                Snap(_hoveredInteractable);
                return; 
            }
        }
    }

    public override void _Process(double delta)
    {

        
        if (_sticky && _hoveredInteractable != null)
        {
            _snappedInteractable = _hoveredInteractable;
            _snappedInteractable.GlobalTransform = GlobalTransform;
        }

        if (_snappedInteractable != null)
        {
            DistanceBreak();
        }
    }

    public virtual void Unsnap(bool force=true)
    {
        if (_snappedInteractable == null) return;
        
        if (CanUnsnap == false  && !force) return; 

        Interactable interactable = _snappedInteractable;

        _hoveredInteractable = null;
        _snappedInteractable = null;

        interactable.Freeze = interactable.InitFreeze;
        interactable.Reparent(interactable.InitParent, true);
        interactable.Owner = interactable.InitParent;

        EmitSignal("OnUnSnap");
    }

    public virtual void Snap(Interactable interactable)
    {

        Connect(interactable);

        _snappedInteractable = interactable;
        interactable.Reparent(this, true);

        tween = GetTree().CreateTween();

        tween.SetParallel(true);
        tween.SetEase(_easeType);
        tween.SetTrans(_transType);
        tween.TweenProperty(_snappedInteractable, "position", Vector3.Zero, _tweenTime);
        tween.TweenProperty(_snappedInteractable, "rotation", Vector3.Zero, _tweenTime);

        _snappedInteractable.Freeze = true;
        EmitSignal("OnSnap", interactable);
    }

    private void Entered(Node3D body)
    {
        if (!Util.NodeIs(body, typeof(Interactor)) || _snappedInteractable != null) return;

        Interactor interactor = (Interactor)body;

        // return if not grabbing 
        if (interactor._grabbedInteractable == null || _hoveredInteractable != null) return;


        // return if parent is type interactable 
        if (Util.NodeIs(interactor._grabbedInteractable.Owner, typeof(Interactable))) return;

        // return if grabbed itneractable is a child of a snapzone 
        if (Util.NodeIs(interactor._grabbedInteractable.GetParent(), typeof(InteractableSnapZone))) return;

        // return if no grab or parent is the grabbed interatable
        if (interactor._grabbedInteractable == null || GetParent() == interactor._grabbedInteractable) return;

        Interactable hovered = (Interactable)interactor._grabbedInteractable;

        bool allowed = false;
        if (AllowedGroups != null)
        {
            foreach (String group in hovered.GetGroups())
            {
                if (AllowedGroups.Contains(group))
                {
                    allowed = true;
                }
            }

        }
        else
        {
            allowed = true;
        }
        if (!allowed) return;

        _hoveredInteractable = (Interactable)interactor._grabbedInteractable;

        Connect(_hoveredInteractable);

        if (_snapMode == SnapMode.OnEnter)
        {
            if (!_sticky)
            {
                _hoveredInteractable.FullDrop();
            }

            Snap(_hoveredInteractable);
        }
    }

    protected void DistanceBreak()
    {
        if (_snappedInteractable == null) return;

        if (!_snappedInteractable.IsGrabbed()) return;

        Interactor interactor = null;
        if (_snappedInteractable.GetPrimaryInteractor() != null)
        {
            interactor = _snappedInteractable.GetPrimaryInteractor();
        }
        else if (_snappedInteractable.GetSecondaryInteractor() != null)
        {
            interactor = _snappedInteractable.GetSecondaryInteractor();
        }

        float distance = interactor.GlobalPosition.DistanceTo(GlobalPosition);
        if (distance > _breakDistance)
        {
            Unsnap();
        }
    }
    public void Exited(Node3D area)
    {
        if (_hoveredInteractable == null || !CanUnsnap) return;

        Disconnect((Interactable)_hoveredInteractable);

        if (area == _hoveredInteractable.PrimaryInteractor)
        {
            if (_sticky && _snappedInteractable != null)
            {
                Unsnap();
            }
        }
    }

    private void OnDropped()
    {
        if (_hoveredInteractable != null && _snapMode == SnapMode.OnDrop)
        {
            CallDeferred("Snap", _hoveredInteractable);
        }
    }

    private void OnGrabbed(Interactable interactable, Interactor interactor)
    {

        if (IsInstanceValid(tween) && tween.IsRunning())
        {
            tween.Stop();
        }

        if (Locked)
        {
            return;
        }

        if (!_sticky)
        {
            Unsnap();
        }

        if (_snapMode != SnapMode.OnDrop)
        {
            Disconnect(interactable);
        }
    }


    private List<Interactable> Hovered()
	{
		Array<Node3D> bodies = GetOverlappingBodies();
		List<Interactable> interactables = new List<Interactable>();

		foreach (Node3D node in bodies)
		{
			if (Util.NodeIs(node, typeof(Interactable)))
			{
				interactables.Add((Interactable)node);
			}
		}
		

		// sort closest hovered interactable 
		interactables = interactables.OrderBy(x => x.GlobalPosition.DistanceTo(GlobalPosition)).ToList();

		return interactables;

	}

    private void Connect(Interactable interactable)
    {
        Action dropAction = OnDropped;
        Action<Interactable, Interactor> grabAction = OnGrabbed;
        bool dropConnected = interactable.IsConnected("OnFullDropped", Callable.From(dropAction));
        bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

        if (!dropConnected)
        {
            interactable.Connect("OnFullDropped", Callable.From(dropAction));
        }
        if (!grabConnected)
        {
            interactable.Connect("OnGrabbed", Callable.From(grabAction));
        }
    }

    private void Disconnect(Interactable interactable)
    {
        if (interactable == null) return;

        Action dropAction = OnDropped;
        Action<Interactable, Interactor> grabAction = OnGrabbed;
        bool dropConnected = interactable.IsConnected("OnFullDropped", Callable.From(dropAction));
        bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

        if (dropConnected)
        {
            interactable.Disconnect("OnFullDropped", Callable.From(dropAction));
        }
        if (grabConnected)
        {
            interactable.Disconnect("OnGrabbed", Callable.From(grabAction));
        }
    }
}
