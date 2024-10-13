using Godot;
using System;

namespace NXR; 

[Tool]
[GlobalClass]
public partial class InputRotator : Node3D
{
    [Export] public Node3D Target; 
    [Export] private Vector3 _start = new();
    [Export]private Vector3 _end = new();

    [ExportGroup("tool settings")]
    [Export] private bool _setStart = false;
    [Export] private bool _setEnd = false;
    [Export] private bool _goStart = false;
    [Export] private bool _goEnd = false;



    public float rotationDelta = 0.0f;


    public override void _Ready()
    {
        Target = this; 
    }

    public override void _Process(double delta)
    {
      if (Engine.IsEditorHint()) { 

            if (_setStart)
            {
                _start = Target.Rotation;
                _setStart = false;
            }
            if (_setEnd)
            {
                _end = Target.Rotation;
                _setEnd = false;
            }

            if (_goStart)
            {
                Target.Rotation = _start; 
                _goStart = false;
            }
            if (_goEnd)
            {
                Target.Rotation = _end; 
                _goEnd = false;
            }
            return; 
        }
        
        if (Target == null) return;
        
        Target.Basis = Basis.FromEuler(_start).Slerp(
            Basis.FromEuler(_end),
            rotationDelta
        ); 
    }
}
