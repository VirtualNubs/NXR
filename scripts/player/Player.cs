using Godot;
using NXR;


namespace NXRPlayer; 

public partial class Player : CharacterBody3D
{
    [Export]
    private DominantHand _dominantHand = DominantHand.Left;

    [Export]
    private Controller _leftController;
    [Export]
    private Controller _rightController;

    [Export]
    private bool _gravityEnabled = true; 


    private Camera3D _camera;
    private RayCast3D _groundRay = new(); 

    private SphereShape3D _headSphereShape = new(); 
    private CollisionShape3D _headCollisionShape = new();

    [Export]
    private float _gravityMultiplier = 1.0f; 

    public override void _Ready()
    {
        if (GetViewport().GetCamera3D().GetClass() == "XRCamera3D")
        {
            _camera = GetViewport().GetCamera3D(); 
        }

        AddChild(_groundRay);
        ConfigureHeadShape(); 
    }


    public override void _Process(double delta)
    {
        _headCollisionShape.GlobalTransform = _camera.GlobalTransform;
        _groundRay.GlobalPosition = _camera.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {

        if (IsOnGround())
        {
            Grounder(); 
        }
        else
        {
            Vector3 gravity = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");
            Accelerate(gravity * _gravityMultiplier);
        }

        MoveAndSlide(); 
    }

    public void ApplyDampening(Vector3 velocity, float amount)
    {
        Accelerate((-1 * amount * Velocity.Length()) * velocity.Normalized());
    }


    public void Accelerate(Vector3 vel)
    {
        Velocity += vel; 
    }

    public Controller GetDominantController()
    {
        if (_dominantHand == DominantHand.Left) return _leftController;
        return _rightController; 
    }

    public Controller GetSecondaryController()
    {
        if (_dominantHand == DominantHand.Left) return _rightController;
        return _leftController;
    }

    public Vector2 GetDominantJoyAxis()
    {
        return GetDominantController().GetVector2("primary"); 
    }

    public Vector2 GetSecondaryJoyAxis()
    {
        return GetSecondaryController().GetVector2("primary");
    }

    public bool IsOnGround()
    {
        return _groundRay.IsColliding(); 
    }


    public Camera3D GetCamera()
    {
        return GetViewport().GetCamera3D(); 
    }

    private void Grounder()
    {
        if (!_groundRay.IsColliding()) return;


        Vector3 pos = GlobalPosition;
        Vector3 camPos = _camera.GlobalPosition;
        float castDistance = Mathf.Abs(pos.Y - camPos.Y);
        float castOffset = 0.2f;


        Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
        _groundRay.TargetPosition = Vector3.Down * (castDistance + castOffset);

        Vector3 newPos = new Vector3(GlobalPosition.X, _groundRay.GetCollisionPoint().Y, GlobalPosition.Z); 
        GlobalPosition = GlobalPosition.Lerp(
            newPos,
            0.3f
        ); 

    }

    private void ConfigureHeadShape()
    {
        if (_headCollisionShape.GetParent() == null)
        {
            AddChild(_headCollisionShape); 
        }

        _headSphereShape.Radius = 0.15f;
        _headCollisionShape.Shape = _headSphereShape; 
    }
}
