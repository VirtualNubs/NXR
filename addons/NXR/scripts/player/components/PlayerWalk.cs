using Godot;
using System;

namespace NXRPlayer; 

[GlobalClass]
public partial class PlayerWalk : PlayerBehaviour
{
    [Export]
    private float _walkSpeed = 50.0f;

    public override void _PhysicsProcess(double delta)
    {
        if (!_player.IsOnGround()) { return; }


        this._player.Accelerate(WalkDirection() * _walkSpeed * (float)delta);
        this._player.ApplyDampening(new Vector3(_player.Velocity.X, 0, _player.Velocity.Z), 0.1f);
    }

    private Vector3 WalkDirection()
    {
        Vector2 axis = _player.GetSecondaryJoyAxis();
        float axisLength = _player.GetSecondaryJoyAxis().Length(); 
        Vector3 right = new Vector3(GetMovementBasis().X.X, 0, GetMovementBasis().X.Z) * axis.X;
        Vector3 forward = new Vector3(-GetMovementBasis().Z.X, 0, -GetMovementBasis().Z.Z) * axis.Y;

        Vector3 movementDirection = (right + forward).Normalized() * axisLength;

        return movementDirection.Slide(_player.GetGroundNormal()); 
    }

    private Basis GetMovementBasis()
    {
        return this._player.GetSecondaryController().GlobalTransform.Basis; 
    }
}
