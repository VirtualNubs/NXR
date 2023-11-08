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
        BodyExited += Exited;
        OnUnSnap += Unsnapped;

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
        if (_sticky && _snappedInteractable != null)
        {
            if (_snappedInteractable.GetParent() != this) return;

            _snappedInteractable.GlobalTransform = GlobalTransform;
        }

        if (_snappedInteractable != null && _snappedInteractable.IsInsideTree())
        {
            DistanceBreak();
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

        // allow some time after reparenting to reset
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        EmitSignal("OnUnSnap");

    }

    public virtual void Snap(Interactable interactable)
    {
        if (!interactable.IsInsideTree()) return; 
        
        Connect(interactable);

        _snappedInteractable = interactable;
        _snappedInteractable.Freeze = true;

        if (interactable.IsInsideTree()) { 
            _snappedInteractable.Reparent(this);
        }   

        tween = GetTree().CreateTween();

        tween.SetParallel(true);
        tween.SetEase(_easeType);
        tween.SetTrans(_transType);
        tween.TweenProperty(_snappedInteractable, "position", Vector3.Zero, _tweenTime);
        tween.TweenProperty(_snappedInteractable, "rotation", Vector3.Zero, _tweenTime);

        EmitSignal("OnSnap", interactable);
    }

    private void Entered(Node3D body)
    {
        if (!CanSnap) return;
        
        if (!Util.NodeIs(body, typeof(Interactable)) || _snappedInteractable != null) return;

        if (Util.NodeIs(body.GetParent(), typeof(InteractableSnapZone))) return;

        if (_hoveredInteractable != null) return;

        Interactable hovered = (Interactable)body;

        if (!hovered.IsGrabbed()) return;

        if (!InGroup(hovered)) return;

        _hoveredInteractable = (Interactable)body;

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

        float distance = interactor.GlobalPosition.DistanceTo(GlobalPosition);
        if (distance > _breakDistance)
        {
            Unsnap(); 
        }
    }

    public void Exited(Node3D body)
    {
        if (_hoveredInteractable == null || !CanUnsnap) return;

        //Disconnect((Interactable)body);
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

    private void Unsnapped()
    {
        Disconnect(_snappedInteractable);
        _hoveredInteractable = null;
        _snappedInteractable = null;
    }
}
