using Godot;
using Godot.Collections;
using NXR;
using NXRInteractable;
using System;
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


    [ExportGroup("Tween Settings")]
    [Export]
    private float _tweenTime = 0.1f;
    [Export]
    private Tween.EaseType _easeType;
    [Export]
    private Tween.TransitionType _transType;


    private Interactable _snappedInteractable = null;
    private Interactable _hoveredInteractable;

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


        if (Util.NodeIs(GetChild(0), typeof(Interactable)))
        {
            Snap((Interactable)GetChild(0));
        }
    }

    public void Unsnap()
    {
        if (_snappedInteractable == null) return;
        Interactable interactable = _snappedInteractable; 

        _snappedInteractable = null;
        EmitSignal("OnUnSnap");
        interactable.Freeze = interactable.InitFreeze; 
        interactable.Reparent(interactable.InitParent, true);
        interactable.Owner = interactable.InitParent;
    }

    public void Snap(Interactable interactable)
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
        if (interactor._grabbedInteractable == null) return;

     
        // return if parent is type interactable 
        if (Util.NodeIs(interactor._grabbedInteractable.Owner, typeof(Interactable))) return;

        // return if no grab or parent is the grabbed interatable
        if (interactor._grabbedInteractable == null || GetParent() == interactor._grabbedInteractable) return;

        _hoveredInteractable = (Interactable)interactor._grabbedInteractable;

        if (AllowedGroups != null)
        {
            foreach (String group in _hoveredInteractable.GetGroups())
            {
                if (AllowedGroups.Contains(group))
                {
                    continue; 
                }
                else
                {
                    return;
                }
            }
        }
    
        Connect(_hoveredInteractable);

        if (_snapMode == SnapMode.OnEnter)
        {   
            _hoveredInteractable.FullDrop();
            Snap(_hoveredInteractable);
        }
    }

    public void Exited(Node3D area)
    {
        if (_hoveredInteractable == null) return; 

        if (area == _hoveredInteractable.PrimaryInteractor || area == _hoveredInteractable.SecondaryInteractor) { 
            Disconnect((Interactable)_hoveredInteractable); 
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

        if (Locked) { 
            return; 
        }
        
        Unsnap();

        if (_snapMode != SnapMode.OnDrop) {
            Disconnect(interactable); 
        }
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
