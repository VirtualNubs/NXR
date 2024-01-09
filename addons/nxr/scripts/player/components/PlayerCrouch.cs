using Godot;
using NXRPlayer;
using System;

[GlobalClass]
public partial class PlayerCrouch : PlayerBehaviour
{
	[Export]
	private float _threshold = 0.5f; 

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector3 camPos = _player.ToLocal(_player.GetCamera().GlobalPosition);
		Vector3 newPos = _player.GetXROrigin().Position; 
		float camOffset = _player.GetXROrigin().Position.Y - camPos.Y;
		float max = _player.GetPlayerHeight() + camOffset; 
		float min = 0.5f + camOffset; 

		newPos.Y = Mathf.Clamp(newPos.Y, min, max);  
		if (!_player.IsOnCeiling()) { 
			newPos.Y += GetJoyInput() * (float)delta;
		}
		_player.GetXROrigin().Position = newPos; 
	}

	private float GetJoyInput() { 
		if (Mathf.Abs(_player.GetDominantJoyAxis().Y) < _threshold) return 0f; 

		return _player.GetDominantJoyAxis().Y; 
	}
}
