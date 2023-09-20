using Godot;
using System;

namespace NXRPlayer;

[GlobalClass]
public partial class PlayerTurn : PlayerBehaviour
{
    [Export]
    private RotationMode _rotationMode = RotationMode.Snap;

    [Export]
    private float _smoothTurnSpeed = 200;

    [Export]
    private int _snapTurnDegrees = 15; 

    [Export]
    float _threshold = 0.5f;

    private bool _snapQueued = true; 

    public override void _Ready()
    {
        _player = (Player)GetParent(); 
    }

    public override void _PhysicsProcess(double delta)
    {

        if (_rotationMode == RotationMode.Snap && _snapQueued)
        {
            _snapQueued = false;
            Rotate( Mathf.DegToRad((float)_snapTurnDegrees));
        } 

        if (_rotationMode == RotationMode.Smooth)

        {
            Rotate((float)_smoothTurnSpeed);
        }

        if (!_snapQueued && (Mathf.Abs(_player.GetDominantJoyAxis().X) < _threshold))
        {
            _snapQueued = true;
        }
    }

    public void Rotate(float speed)
    {
        Transform3D t1 = new Transform3D();
        Transform3D t2 = new Transform3D();
        Transform3D rot = new Transform3D();


        t1.Basis = Basis.Identity;
        t1.Origin = _player.GetCamera().Transform.Origin;
        t2.Basis = Basis.Identity;
        t2.Origin = -_player.GetCamera().Transform.Origin;
        rot.Basis = Basis.Identity;

        if (_rotationMode == RotationMode.Snap)
        {
            rot = rot.Rotated(-Vector3.Up, _player.GetDominantJoyAxis().X);
        }
        else
        {
            rot = rot.Rotated(new Vector3(0, -1, 0), _player.GetDominantJoyAxis().X / 100.0f);
        }

        if (Mathf.Abs(_player.GetDominantJoyAxis().X) > _threshold)
        {
            _player.Transform = (_player.Transform * t1 * rot * t2).Orthonormalized();
        } 
    }

}


