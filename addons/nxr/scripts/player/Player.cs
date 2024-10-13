using Godot;
using Godot.Collections;
using NXR;


namespace NXRPlayer; 

[GlobalClass]
public partial class Player : CharacterBody3D
{   

    #region Exported
    [Export] public PlayerSettings PlayerSettings { get; set; } 
    [Export] private float _playerHeight = 1.8f; 
    [Export] public DominantHand _dominantHand = DominantHand.Left;
    [Export] private Controller _leftController;
    [Export] private Controller _rightController;
    [Export] private XROrigin3D _xrOrigin; 
    [Export] private bool _gravityEnabled = true;
    [Export] private float _gravityMultiplier = 1.0f; 


    [ExportGroup("Collider Settings")]
    [Export] private bool _enableHeadCollider = true; 
    [Export] private bool _enableBodyCollider = true; 



    [ExportGroup("Step Settings")]
    [Export] private float _stepHeight = 0.4f;


    [Export(PropertyHint.Range, "0.01, 1.0")]
    private float _stepSmoothing = 0.2f; 
    #endregion


    #region private
    private Camera3D _camera;
    private RayCast3D _groundRay = new(); 

    private SphereShape3D _headSphereShape = new(); 
    private CollisionShape3D _headCollisionShape = new();

    private CylinderShape3D _bodyShape = new();
    private CollisionShape3D _bodyCollisionShape = new();

    private Node3D lastGroundNode; 
    private Transform3D lastGroundXform; 
    #endregion


    public override void _Ready()
    {

        if (PlayerSettings.SettingsExist("default_player_settings")) { 
            PlayerSettings = PlayerSettings.LoadSettings(); 
        } else { 
            PlayerSettings = PlayerSettings = new PlayerSettings();
        }
        

        if (GetViewport().GetCamera3D().GetClass() == "XRCamera3D")
        {
            _camera = GetViewport().GetCamera3D(); 
        }

        AddChild(_groundRay);
        ConfigureCollisionShapes(); 
    }


    public override void _Process(double delta)
    {
        _headCollisionShape.GlobalTransform = _camera.GlobalTransform;
        _groundRay.GlobalPosition = _camera.GlobalPosition;


        float bodyShapeHeight = Mathf.Abs(GlobalPosition.Y - _camera.GlobalPosition.Y) - _stepHeight;
        bodyShapeHeight = Mathf.Clamp(bodyShapeHeight, 0.1f, 10.0f); 
        _bodyShape.Height = bodyShapeHeight; 
        Vector3 bodyPos = new Vector3(GetCamera().GlobalPosition.X, GetCamera().GlobalPosition.Y - (_bodyShape.Height / 2), GetCamera().GlobalPosition.Z);

        _bodyCollisionShape.GlobalPosition = bodyPos; 

        if(!_enableBodyCollider) { 
            _bodyCollisionShape.Disabled = true; 
        } else { 
            _bodyCollisionShape.Disabled = false; 
        }
        
        if(!_enableHeadCollider) { 
            _headCollisionShape.Disabled = true; 
        } else { 
            _headCollisionShape.Disabled = false; 
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (IsOnGround())
        {
            Grounder();
        }
        else
        {
            float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity") / 30f; 
            Vector3 gravityVector = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector") * Mathf.Abs(gravity);

            if (_gravityEnabled) {
                Accelerate(gravityVector  * _gravityMultiplier);
            }
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


    public void CenterOnNode(Node3D node) {
        Vector3 bodyOffset = GlobalPosition - GetCamera().GlobalPosition; 

        GlobalPosition = node.GlobalPosition + bodyOffset; 
    }


    public Controller GetDominantController()
    {
        if (PlayerSettings.DominantHand == DominantHand.Left) return _leftController;
        return _rightController; 
    }


    public Controller GetSecondaryController()
    {
        if (PlayerSettings.DominantHand== DominantHand.Left) return _rightController;
        return _leftController;
    }


    public Vector2 GetDominantJoyAxis()
    {
        if (GetDominantController() == null) return Vector2.Zero; 
        return GetDominantController().GetVector2("primary"); 
    }


    public Vector2 GetSecondaryJoyAxis()
    {
        if (GetSecondaryController() == null) return Vector2.Zero;
        return GetSecondaryController().GetVector2("primary");
    }


    public Vector3 GetGroundNormal()
    {
        return _groundRay.GetCollisionNormal(); 
    }


    public XROrigin3D GetXROrigin() { 
        return _xrOrigin; 
    }


    public float GetPlayerHeight() { 
        return _playerHeight; 
    }


    public bool IsOnGround()
    {
        return _groundRay.IsColliding() || IsOnFloor(); 
    }


    public Camera3D GetCamera()
    {
        return GetViewport().GetCamera3D(); 
    }


    public Array<StringName> GetGroundGroups() { 
        Node3D col = (Node3D)_groundRay.GetCollider(); 
        return col.GetGroups();  
    }


    private void Grounder()
    {

        if (IsOnCeiling()) return; 
        
        Vector3 pos = GlobalPosition;
        Vector3 camPos = _camera.GlobalPosition;
        float castDistance = Mathf.Abs(pos.Y - camPos.Y);
        float castOffset = 0.2f;


        Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
        _groundRay.TargetPosition = Vector3.Down * (castDistance + castOffset);

        Vector3 newPos = new Vector3(GlobalPosition.X, _groundRay.GetCollisionPoint().Y, GlobalPosition.Z); 
        GlobalPosition = GlobalPosition.Lerp(
            newPos,
            _stepSmoothing
        );
    }


    public void SetPlayerHeight() { 
        _playerHeight = GetCamera().Position.Y; 
    }


    public void SetDominantHand(DominantHand hand) { 
        _dominantHand = hand; 
    }


    private void ConfigureCollisionShapes()
    {
        if (_headCollisionShape.GetParent() == null)
        {
            AddChild(_headCollisionShape);
        }
        if (_bodyCollisionShape.GetParent() == null)
        {
            AddChild(_bodyCollisionShape);
        }

        _headSphereShape.Radius = 0.15f;
        _headCollisionShape.Shape = _headSphereShape;

        _bodyCollisionShape.Shape = _bodyShape; 
        _bodyShape.Height = 0.8f;
        _bodyShape.Radius = 0.15f; 
    }
}
