using Godot;
using NXRFirearm;
using NXRInteractable;
using NXR;

[Tool]
[GlobalClass]
public partial class FirearmSlide : FirearmClampedXform
{
    #region Exported
    [Export]private bool _setBackOnFire = false; 
    [Export]private bool _setBackOnEmpty = false; 
    [Export] private string _releaseAction = ""; 
    #endregion


    #region Private
    protected bool back = false; 
    private Transform3D _relativeGrabXform; 
    protected Transform3D _relativeXform = new(); 
    private bool lockedBack = false; 
    #endregion


    #region Signals 
    [Signal] public delegate void SlideBackEventHandler(); 
    [Signal] public delegate void SlideForwardEventHandler(); 
    #endregion



    public override void _Ready()
    {
        base._Ready(); 
        
        if (Firearm == null) return; 

        Firearm.OnFire += OnFire;
        Firearm.OnChambered += Chambered; 
        Firearm.TryChamber += TryChambered; 

        OnDropped += OnDrop;
        OnGrabbed += Grabbed;
    }


    public override void _Process(double delta)
    {
        RunTool(); 
      
        if (Firearm == null) return;
        
        if (AtEnd() && !back && IsGrabbed()) {
            
            if (Firearm.Chambered) { 
                Firearm.EmitSignal("TryEject"); 
                Firearm.Chambered = false; 
            } 
            back = true;  
            EmitSignal("SlideBack"); 
        }

        if (!AtEnd() && back) { 
            Firearm.EmitSignal("TryChamber"); 
            back = false; 
            EmitSignal("SlideForward"); 
        }

        if (GetReleaseInput() == true && IsBack()) { 
            Firearm.EmitSignal("TryChamber"); 
        }
    }

    public override void _PhysicsProcess(double delta)
    {

        base._PhysicsProcess(delta); 
        
        if (IsGrabbed())
        {
            Node3D parent = (Node3D)GetParent();
            Transform3D newXform = GetPrimaryInteractor().GlobalTransform * _relativeXform; 
            Vector3 newPos = parent.ToLocal(newXform.Origin);
            newPos= newPos.Clamp(StartXform.Origin, EndXform.Origin);
            Position = newPos;
        } 

    }


    public bool IsBack() { 
        return Position.IsEqualApprox(EndXform.Origin); 
    }


    public bool IsForward() { 
        return Position.IsEqualApprox(StartXform.Origin); 
    }


    public void OnFire()
    {
        if (_setBackOnFire)
        {
            Position = EndXform.Origin;
        }
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        ReturnTween();
    }
    
    private void Grabbed(Interactable interactable, Interactor interactor) { 

        if (interactor == GetPrimaryInteractor()) {
            _relativeXform = interactor.GlobalTransform.AffineInverse() * GlobalTransform; 
        }
    }


     private void TryChambered() { 
        if (IsBack() && !Firearm.Chambered ) { 
            ReturnTween();
        }
    }
    private void Chambered() { 
        ReturnTween();
    }
    
    private void ReturnTween()
    {
        if (lockedBack) return; 

        Tween returnTween = GetTree().CreateTween();
        returnTween.TweenProperty(this, "position", StartXform.Origin, 0.1f);
    }


    private bool GetReleaseInput() { 
        if (Firearm.GetPrimaryInteractor() == null) return false; 
        return Firearm.GetPrimaryInteractor().Controller.ButtonOneShot(_releaseAction); 
    }
}
