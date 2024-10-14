using Godot;
using NXR;
using NXRFirearm;
using System;

[GlobalClass]
public partial class FirearmRayHitSpawner : Node
{
	[Export] private PackedScene _scene; 
	[Export] private FirearmRay _ray; 

	
	public override void _Ready()
	{
		if (_ray == null && Util.NodeIs(GetParent(), typeof(FirearmRay))) { 
			_ray = (FirearmRay)GetParent(); 
			_ray.OnHit += OnHit; 
		}
	}

	private void OnHit(Node3D node, Vector3 at) { 
		Node3D inst = (Node3D)_scene.Instantiate(); 
		_ray.AddChild(inst); 

		inst.GlobalPosition = at; 
		inst.LookAt(_ray.GlobalPosition, Vector3.Up); 
	}
}
