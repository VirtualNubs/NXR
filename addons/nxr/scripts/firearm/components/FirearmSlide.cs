using Godot;
using NXRFirearm;
using NXRInteractable;
using NXR;

[Tool]
[GlobalClass]
public partial class FirearmSlide : FirearmMovable
{
    [Export]
    private bool _setBackOnFire = false; 
    [Export]
    private bool _setBackOnEmpty = false; 

    [Export]
    private Node3D  _firearmNode; 
    protected Firearm _firearm = null; 
    protected bool back = false; 
    private Transform3D _relativeGrabXform; 

    protected Transform3D _relativeXform = new(); 

    private bool lockedBack = false; 

    public override void _Ready()
    {
        base._Ready(); 
        
        if (Util.NodeIs(_firearmNode, typeof(Firearm)))
        {
            _firearm = (Firearm)_firearmNode;
        }


         if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }
        
        if (_firearm == null) return; 

        _firearm.OnFire += OnFire;
        _firearm.OnChambered += Chambered; 
        _firearm.TryChamber += TryChambered; 

        this.OnDropped += OnDrop;
        this.OnGrabbed += Grabbed;
    }

    public override void _Process(double delta)
    {
        RunTool(); 
      
        if (_firearm == null) return;
        
        if (AtEnd() && !back && IsGrabbed()) {
            
            if (_firearm.Chambered) { 
                _firearm.EmitSignal("TryEject"); 
                _firearm.Chambered = false; 
            } 
            back = true;  
        }

        if (!AtEnd() && back) { 
            _firearm.EmitSignal("TryChamber"); 
            back = false; 
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
        if (IsBack() && !_firearm.Chambered ) { 
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
}
