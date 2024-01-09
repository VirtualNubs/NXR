using Godot;
using System;

public partial class FirearmScopeViewport : SubViewport
{

	[Export]
	private float _enableDistance = 0.5f;

	[Export]
	private Camera3D _camera;  
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float distance = GetTree().CurrentScene.GetViewport().GetCamera3D().GlobalPosition.DistanceTo(_camera.GlobalPosition); 

		if (distance < _enableDistance) { 
			RenderTargetUpdateMode = UpdateMode.Always; 
		} else {
			RenderTargetUpdateMode = UpdateMode.Disabled; 
		 }
	}
}
