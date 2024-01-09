using Godot;
using Godot.Collections;
using NXR;
using NXRInteractable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


[GlobalClass]
public partial class InteractableSnapZone : Area3D
{
    [Export]
    public bool Locked = false;

    [Export]
    public String[] AllowedGroups;

    [Export]
    private SnapMode _snapMode = SnapMode.OnEnter;


    [ExportGroup("Tween Settings")]
    [Export]
    private float _tweenTime = 0.1f;
    [Export]
    private Tween.EaseType _easeType;
    [Export]
    private Tween.TransitionType _transType;


    [ExportGroup("Distance Settings")]
    [Export]
    private float _snapDistance  = 0.08f;
    [Export]
    private float _breakDistance = 0.1f; 


    public Interactable _snappedInteractable = null;
    public Interactable _hoveredInteractable;

    public bool CanSnap = true;
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
        BodyEntered += Entered;

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
        if (_snapMode == SnapMode.Distance)
        {
            if (_snappedInteractable == null && _hoveredInteractable != null)
            {
                DistanceSnap(_hoveredInteractable);
            }
            
            if (_snappedInteractable != null)
            {
                _snappedInteractable.GlobalTransform = GlobalTransform;
                DistanceBreak();
            }
        }
    }

    public virtual async void Unsnap(bool force = true)
    {
        if (_snappedInteractable == null) return;

        if (CanUnsnap == false && !force) return;


        if (_snappedInteractable.IsInsideTree())
        {
            _snappedInteractable.Reparent(_snappedInteractable.InitParent);
        }

        Interactable interactable = _snappedInteractable;

        interactable.Freeze = interactable.InitFreeze;
        interactable.Owner = interactable.InitParent;

        Disconnect(_snappedInteractable);
        _snappedInteractable = null;

        EmitSignal("OnUnSnap");
    }

    public virtual void Snap(Interactable interactable)
    {
        if (!interactable.IsInsideTree()) return;

        Connect(interactable);

        _snappedInteractable = interactable;
        _snappedInteractable.Freeze = true;

        if (interactable.IsInsideTree())
        {
            _snappedInteractable.Reparent(this);
        }

        if (_snapMode != SnapMode.Distance ) {
            SnapTween();
        }

        EmitSignal("OnSnap", interactable);
    }

    private void SnapTween()
    {
        if (_snapMode == SnapMode.Distance) return;

        tween = GetTree().CreateTween();

        tween.SetParallel(true);
        tween.SetEase(_easeType);
        tween.SetTrans(_transType);
        tween.TweenProperty(_snappedInteractable, "position", Vector3.Zero, _tweenTime);
        tween.TweenProperty(_snappedInteractable, "rotation", Vector3.Zero, _tweenTime);
    }

    private void Entered(Node3D body)
    {

        if (!CanSnap) return;

        if (!Util.NodeIs(body, typeof(Interactable)) || _snappedInteractable != null) return;

        if (Util.NodeIs(body.GetParent(), typeof(InteractableSnapZone))) return;


        Interactable hovered = (Interactable)body;

        if (!hovered.IsGrabbed()) return;

        if (!InGroup(hovered)) return;

        _hoveredInteractable = (Interactable)body;
        Connect(_hoveredInteractable);

        if (_snapMode == SnapMode.OnEnter)
        {
           
            _hoveredInteractable.FullDrop();

            Snap(_hoveredInteractable);
        }
    }

    protected void DistanceBreak()
    {

        if (_snappedInteractable == null || !CanUnsnap) return;


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

        float distance = interactor.GlobalPosition.DistanceTo(_snappedInteractable.GlobalPosition);
        if (distance > _breakDistance)
        {
            Unsnap();
            return; 
        }
    }

    protected void DistanceSnap(Interactable interactable)
    {

        if (_snappedInteractable != null) return;


        if (!interactable.IsGrabbed()) return;

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
       
        Disconnect(interactable);
    }


    private bool InGroup(Node3D interactable)
    {
        bool allowed = false;

        if (AllowedGroups != null)
        {
            foreach (String group in interactable.GetGroups())
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

        return allowed;
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
