using Godot;

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
		inst.GlobalPosition = GlobalPosition; 
		
		GetParent().MoveChild(inst, 0);  
	}
}
