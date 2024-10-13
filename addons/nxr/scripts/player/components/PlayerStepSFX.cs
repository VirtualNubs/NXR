using Godot;
using Godot.Collections;
using NXR;
using NXRPlayer;
using System;

[GlobalClass]

public partial class PlayerStepSFX : AudioStreamPlayer3D
{

	private Player _player;

	[Export] private Array<String> _surfaces;
	[Export] private Array<AudioStream> _streams;

	private string _currentSurface = "";


	public override void _Ready()
	{
		if (Util.NodeIs(GetParent(), typeof(Player)))
		{
			_player = (Player)GetParent();
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (_player == null) return;


		Array<StringName> groups = _player.GetGroundGroups();
		
		foreach (StringName surface in _surfaces)
		{

			if (groups.Count <= 0)
			{
				_currentSurface = "";
			}

			if (groups.Contains(surface) && _currentSurface == "") _currentSurface = surface;
		}
	}
}
