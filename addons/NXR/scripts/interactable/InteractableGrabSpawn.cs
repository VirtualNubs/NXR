using Godot;

namespace NXRInteractable; 

[GlobalClass]
public partial class InteractableGrabSpawn : Interactable
{
	[Export]
	public bool Disabled = false; 

	[Export]
	private PackedScene _scene;

    public override void _Ready()
    {
        base._Ready();
		OnGrabbed += Grabbed;  
    }

	private void Grabbed(Interactable interactable, Interactor interactor) { 

		if (Disabled) return; 

		SpawnAndGrab(interactor); 
		
	}

	public void SpawnAndGrab(Interactor interactor) {
		interactor.Drop(); 

		Interactable inst = (Interactable)_scene.Instantiate(); 
		GetParent().AddChild(inst); 
		inst.GlobalPosition = this.GlobalPosition; 
		interactor.Grab(inst); 
	}
	
}
