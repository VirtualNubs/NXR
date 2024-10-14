using Godot;
using System;

public partial class FirearmScopeViewport : SubViewport
{

	[Export] private float _enableDistance = 0.5f;
	[Export] private Camera3D _camera;  

	public override void _Process(double delta)
	{

		if (_camera == null) return; 
		
		float distance = GetTree().CurrentScene.GetViewport().GetCamera3D().GlobalPosition.DistanceTo(_camera.GlobalPosition); 

		if (distance < _enableDistance) { 
			RenderTargetUpdateMode = UpdateMode.Always; 
		} else {
			RenderTargetUpdateMode = UpdateMode.Disabled; 
		 }
	}
}
