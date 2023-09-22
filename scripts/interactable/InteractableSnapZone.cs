using Godot;
using NXR;
using System;
using Godot.Collections;

[GlobalClass]
public partial class InteractableSnapZone : Area3D
{
    [Export]
    private SnapMode _snapMode = SnapMode.OnEnter; 

    [Export]
    private float _unsapDistance = 0.2f;

    //[Export]
    //private float _scaleMultiplier = 1.0f; 

    [ExportGroup("Tween Settings")]
    [Export]
    private float _tweenTime = 0.1f;
    [Export]
    private Tween.EaseType _easeType;
   
    private Interactable _snappedInteractable;
    private Interactable _hoveredInteractable;

    private Vector3 _snappedInitScale; 

    public override void _Ready()
    {
        BodyEntered += Entered; 
        BodyExited += Exited; 
    }

    public override void _Process(double delta)
    {
        if (_snapMode == SnapMode.Sticky) {
            if (_hoveredInteractable != null && _snappedInteractable == null)
            {
                 float dist = _hoveredInteractable.PrimaryInteractor.GlobalPosition.DistanceTo(GlobalPosition) ;

                if (dist < _unsapDistance)
                {
                    Snap(_hoveredInteractable);
                }
            }

            if (IsInstanceValid(_snappedInteractable) && _snappedInteractable.PrimaryInteractor != null)
            {
                float dist = _snappedInteractable.PrimaryInteractor.GlobalPosition.DistanceTo(GlobalPosition);

                if (dist > _unsapDistance)
                {
                    Unsnap();
                }

            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {

        if (_snappedInteractable == null) return; 

            Tween tween = GetTree().CreateTween();

            tween.SetProcessMode(Tween.TweenProcessMode.Physics); 
            tween.SetParallel(true); 
            tween.SetEase(_easeType);
            tween.TweenProperty(_snappedInteractable, "global_position", GlobalPosition, _tweenTime); 
            tween.TweenProperty(_snappedInteractable, "global_rotation", GlobalRotation, _tweenTime); 
            //tween.TweenProperty(_snappedInteractable, "scale", _snappedInitScale * _scaleMultiplier, _tweenTime); 
    }

    public void Unsnap()
    {
        Disconnect(_snappedInteractable);
        _snappedInteractable.Scale = _snappedInitScale; 
        _hoveredInteractable = null;  
        _snappedInteractable = null; 
    }

    public void Snap(Interactable interactable)
    {
        
        _snappedInteractable = interactable;
        _snappedInitScale = interactable.Scale; 
    } 

    private void Entered(Node3D body)
    {
        if (!body.HasMethod("IsInteractable") || _snappedInteractable != null) return;

        Interactable interactableBody = (Interactable)body;

        if (!interactableBody.IsGrabbed()) return; 

        _hoveredInteractable = (Interactable)body;
       
        Connect(_hoveredInteractable); 

        if (_snapMode == SnapMode.OnEnter)
        {
            _hoveredInteractable.FullDrop();
            Snap(_hoveredInteractable);
        }

        if (_snapMode == SnapMode.OnEnter)
        {
            _hoveredInteractable.FullDrop(); 
            Snap(_hoveredInteractable); 
        }
    }

    public void Exited(Node3D body)
    {
        if (!body.HasMethod("IsInteractable")) return;

        if (_snapMode == SnapMode.OnDrop)
        {
            Disconnect(_hoveredInteractable);
        }

        if (body == _hoveredInteractable)
        {
            _hoveredInteractable = null; 
        }

    }

    private void OnDropped(Interactable interactable, Interactor interactor)
    {
        Snap(interactable); 
    }

    private void OnGrabbed(Interactable interactable, Interactor interactor)
    {
        if (_snapMode == SnapMode.OnEnter || _snapMode == SnapMode.OnDrop)
        {
            Unsnap(); 
        }
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
        bool dropConnected = interactable.IsConnected("OnDropped", Callable.From(dropAction));

        if (dropConnected)
        {
            interactable.Disconnect("OnDropped", Callable.From(dropAction));
        }

        Action<Interactable, Interactor> grabAction = OnGrabbed;
        bool grabConnected = interactable.IsConnected("OnGrabbed", Callable.From(grabAction));

        if (grabConnected)
        {
            interactable.Disconnect("OnGrabbed", Callable.From(grabAction));
        }
    }
}
