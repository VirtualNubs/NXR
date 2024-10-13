using Godot;

namespace NXRInteractable; 

[GlobalClass]
public partial class InteractableGrabSpawn : Interactable
{
	[Export] public bool Disabled = false; 
	[Export] private int _maxItems = 1;
	[Export] private PackedScene _scene;


	private Interactable _lastSpawned = null;


    public override void _Ready()
    {
        base._Ready();
		OnGrabbed += Grabbed;  

		if (Disabled) Visible = false; 
    }

	private void Grabbed(Interactable interactable, Interactor interactor) { 

		if (Disabled) return; 

		SpawnAndGrab(interactor); 
		
	}

	public void SpawnAndGrab(Interactor interactor) {


		if (_lastSpawned != null) {
			_lastSpawned.QueueFree(); 
		}

		
		interactor.Drop(); 

		Interactable inst = (Interactable)_scene.Instantiate(); 
		GetParent().AddChild(inst); 
		inst.GlobalPosition = this.GlobalPosition; 
		interactor.Grab(inst); 
		_lastSpawned = inst; 

	}

	public Interactable GetLastSpawned() { 
		return _lastSpawned; 
	}
}
