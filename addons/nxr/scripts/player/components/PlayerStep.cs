using Godot;
using NXR;
using NXRPlayer;
using System;

public partial class PlayerStep : ShapeCast3D
{
	[Export]
	private float _smoothing = 0.5f; 

	private Player _player; 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Util.NodeIs(GetParent(), typeof(Player))) { 
			_player = (Player)GetParent(); 
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player == null) return; 

		Vector3 vel = _player.Velocity.Normalized(); 
		Vector3 camPos = _player.GetCamera().GlobalPosition;
		GlobalPosition = new Vector3(camPos.X, 0.4f ,camPos.Z); 

		bool colliding = IsColliding(); 
		Vector3 normal = Vector3.Zero; 
		float angle = 0; 

		if (colliding) { 
			normal = GetCollisionNormal(GetCollisionCount() - 1);
			angle = normal.AngleTo(Vector3.Up); 
			GD.Print(normal); 
		}


		if (colliding && angle < 50) { 
			float newY = GetCollisionPoint(0).Y; 
			Vector3 newPos = new Vector3(_player.GlobalPosition.X, newY, _player.GlobalPosition.Z); 

			_player.GlobalPosition = newPos; 
		}
	}
}
