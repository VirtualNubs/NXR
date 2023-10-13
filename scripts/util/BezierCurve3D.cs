using Godot;

[Tool]
[GlobalClass]

public partial class BezierCurve3D : Path3D
{
    [Export]
    public bool update = true;

    [Export]
    public float UpdateTime = 0.1f; 

    [Export]
    public int Resolution = 10;

    [Export]
    private Vector3 StartPoint = Vector3.Zero;

    [Export]
    public Vector3 MidPoint = new Vector3(0, 0.5f, -0.5f); 

    [Export]
    public Vector3 EndPoint = Vector3.Forward;


    [ExportGroup("Animate")]
    [Export]
    public float SinSpeed = 0.0f;
    [Export(PropertyHint.Range, "0.0, 5")]
    public float SinAmplitude = 0.0f;
    [Export]
    public float CosSpeed = 0.0f;
    [Export(PropertyHint.Range, "0.0, 5")]
    public float CosAmplitude = 0.0f;


    private float _sinTime = 0.0f;
    private float _cosTime = 0.0f;

    [ExportGroup("Debug")]
    [Export]
    private Color _debugColor = new Color();

    [Export(PropertyHint.Range, "0.0, 5")]
    private float _debugScale = 0.05f; 

    public override void _Process(double delta)
    {

        if (!update) return;

        _sinTime += (float)delta; 
        _cosTime += (float)delta; 

        if (Engine.IsEditorHint())
        {

            if (GetNode("DebugDraws") == null)
            {
                Node3D debugDraws = new Node3D();
                debugDraws.Name = "DebugDraws"; 

                AddChild(debugDraws);
            } else
            {
                Node3D debugDraws = (Node3D)GetNode("DebugDraws");
                foreach (Node3D chilld in debugDraws.GetChildren())
                {
                    chilld.QueueFree(); 
                }
            }

            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0:
                        MeshInstance3D debug1 = SpawnDebug(StartPoint, GetNode("DebugDraws"));
                        break;
                    case 1:
                        MeshInstance3D debug2 = SpawnDebug(MidPoint, GetNode("DebugDraws"));
                        break;
                     case 2:
                        MeshInstance3D debug3 = SpawnDebug(EndPoint, GetNode("DebugDraws"));
                        break;
                }
            }
        }

        UpdateCurve(); 
    }


    public async void UpdateCurve()
    {

        Curve.ClearPoints();
        Curve.UpVectorEnabled = false;
        for (int i = 0; i < Resolution; i++)
        {
            float t = (float)i / (Resolution - 1);

            Vector3 point = GetCurve(t);
            Vector3 sin = Vector3.Up * Mathf.Sin(i + (SinSpeed * _sinTime)) * SinAmplitude;
            Vector3 cos = Vector3.Right * Mathf.Cos((i + _cosTime) * CosSpeed) * CosAmplitude;

            Curve.AddPoint(point + sin + cos);
        }
    }
    public MeshInstance3D SpawnDebug(Vector3 position, Node parent)
    {
        StandardMaterial3D mat = new(); 
        MeshInstance3D mesh = new MeshInstance3D();
        SphereMesh sphere = new SphereMesh();

        sphere.Material = mat;
        sphere.Material.Set("transparency", 1); 
        sphere.Radius = _debugScale;
        sphere.Height = _debugScale * 2; 
        mat.Set("albedo_color", _debugColor);
        mat.Set("shading_mode", 0);
        mesh.Mesh = sphere; 

        parent.AddChild(mesh);
        mesh.Position = position;

        return mesh; 
    }

    public Vector3 GetCurve(float t)
    {
        Vector3 p0 = StartPoint;
        Vector3 p2 = EndPoint;
        Vector3 p1 = MidPoint;


        Vector3 start = (1f - t) * (1.0f - t) * p0;
        Vector3 mid = 2.0f * (1f -t) * t * p1;
        Vector3 end = t * t * p2;
        
        return start + mid + end; 
    }

}
