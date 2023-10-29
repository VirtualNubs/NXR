using Godot;
using NXR;
using NXRFirearm;
using System;

[GlobalClass]
public partial class FirearmRayHitSpawner : Node
{
	[Export]
	private PackedScene _scene; 
	private FirearmRay _ray; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Util.NodeIs(GetParent(), typeof(FirearmRay))) { 
			_ray = (FirearmRay)GetParent(); 
			_ray.OnHit += OnHit; 
		}
	}

	private void OnHit(Node3D node, Vector3 at) { 
		Node3D inst = (Node3D)_scene.Instantiate(); 
		_ray.AddChild(inst); 

		inst.GlobalPosition = at; 
	}
}
