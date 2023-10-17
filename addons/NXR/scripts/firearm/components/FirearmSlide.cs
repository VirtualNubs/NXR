using Godot;
using NXRFirearm;
using NXRInteractable;
using NXR;

[Tool]
[GlobalClass]
public partial class FirearmSlide : Interactable
{

    [Export]
    protected Vector3 _startPosition;
    [Export]
    protected Vector3 _endPosition;

    [Export]
    private bool _setBackOnFire;

    [ExportGroup("Tool Settings")]
    [Export]
    private bool _setStart;
    [Export]
    private bool _setEnd;
    [Export]
    private bool _goStart;
    [Export]
    private bool _goEnd;

    protected Firearm _firearm = null; 
    protected bool back = false; 
    private Transform3D _relativeGrabXform; 
    
    public override void _Ready()
    {
        base._Ready(); 
        
        if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }
        
        if (_firearm == null) return; 

        _firearm.OnFire += OnFire;
        this.OnDropped += OnDrop;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            if (_setStart)
            {
                _startPosition = Position;
                _setStart = false;
            }
            if (_setEnd)
            {
                _endPosition = Position;
                _setEnd = false;
            }

            if (_goStart)
            {
                Position = _startPosition;
                _goStart = false;
            }
            if (_goEnd)
            {
                Position = _endPosition;
                _goEnd = false;
            }
        }

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
            Node3D parent = (Node3D)GetParent();
            Transform3D grabXform = GetPrimaryRelativeXform(); 
            Vector3 newPos = parent.ToLocal(grabXform.Origin);

            newPos= newPos.Clamp(_startPosition, _endPosition);
            Position =  newPos;
        } 
    }

    public bool IsBack() { 
        return Position.IsEqualApprox(_endPosition); 
    }

    public bool IsForward() { 
        return Position.IsEqualApprox(_startPosition); 
    }

    public void OnFire()
    {
        if (_setBackOnFire)
        {
            Position = _endPosition;
        }

        if (Position.IsEqualApprox(_endPosition))
        {
            ReturnTween();
        }
    }

    public void OnDrop(Interactable interactable, Interactor interactor)
    {
        ReturnTween();
    }
 

    private void ReturnTween()
    {
        Tween returnTween = GetTree().CreateTween();
        returnTween.TweenProperty(this, "position", _startPosition, 0.1f);
    }

}
