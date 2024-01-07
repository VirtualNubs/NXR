using Godot;
using NXR;

[GlobalClass]
public partial class PlayerSpawn : Marker3D
{
	[Export]
	private PackedScene _player; 

	public override void _Ready()
	{
		CallDeferred("spawn"); 
	}

	private void spawn() { 
		Node3D inst = (Node3D)_player.Instantiate(); 

		GetParent().AddChild(inst);
		GetParent().MoveChild(inst, 0);  
		inst.GlobalTransform = GlobalTransform; 

		Util.Recenter(this); 

	}

	private async void Recenter() { 
		float lifetime = Time.GetTicksMsec() / 1000.0f; 
		float delay = 2f; 

		if (lifetime > 2f) delay = 0; 

		await ToSignal(GetTree().CreateTimer(delay), "timeout"); 
		XRServer.CenterOnHmd(XRServer.RotationMode.ResetButKeepTilt, true); 
	}
}
