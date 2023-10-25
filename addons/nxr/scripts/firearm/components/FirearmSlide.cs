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

    protected Firearm _firearm = null; 
    protected bool back = false; 
    private Transform3D _relativeGrabXform; 

    protected Transform3D _relativeXform = new(); 


    public override void _Ready()
    {
        base._Ready(); 
        
        if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }
        
        if (_firearm == null) return; 

        _firearm.OnFire += OnFire;
        _firearm.TryChamber += TriedChamber;

        this.OnDropped += OnDrop;
        this.OnGrabbed += Grabbed;
    }

    public override void _Process(double delta)
    {
        RunTool(); 

        if (_firearm == null) return;
        
        if (IsBack() && !back && IsGrabbed()) {
            back = true;  
        }

        if (IsForward() && back) { 
            back = false; 
            _firearm.EmitSignal("TryChamber"); 
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return; 

        if (IsGrabbed())
        {
            Node3D parent = (Node3D)Target.GetParent();
            Transform3D newXform = GetPrimaryInteractor().GlobalTransform * _relativeXform; 
            Vector3 newPos = parent.ToLocal(newXform.Origin);

            newPos= newPos.Clamp(StartXform.Origin, EndXform.Origin);
            Target.Position = newPos;
        } 
    }

    public bool IsBack() { 
        return Target.Position.IsEqualApprox(EndXform.Origin); 
    }

    public bool IsForward() { 
        return Target.Position.IsEqualApprox(StartXform.Origin); 
    }

    public void OnFire()
    {
        if (_setBackOnFire)
        {
            Target.Position = EndXform.Origin;
        }


        if (_setBackOnEmpty && _firearm.Chambered == false) return; 

        if (Target.Position.IsEqualApprox(EndXform.Origin))
        {
            ReturnTween();
        }
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        ReturnTween();
    }
    
    private void Grabbed(Interactable interactable, Interactor interactor) { 

        if (interactor == GetPrimaryInteractor()) {
            _relativeXform = interactor.GlobalTransform.AffineInverse() * Target.GlobalTransform; 
        }
    }

    private void TriedChamber() { 
        ReturnTween(); 
    }
    
    private void ReturnTween()
    {
        Tween returnTween = GetTree().CreateTween();
        returnTween.TweenProperty(Target, "position", StartXform.Origin, 0.1f);
    }

}
