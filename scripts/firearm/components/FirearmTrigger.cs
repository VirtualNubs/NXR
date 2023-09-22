using Godot;
using NXR;
using NXRFirearm;

[Tool]
[GlobalClass]
public partial class FirearmTrigger : InputRotator
{

    private Firearm _firearm;

    public override void _Ready()
    {
        if (GetParent() != null)
        {
            _firearm = (Firearm)GetParent();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Engine.IsEditorHint()) return; 
        if (_firearm.PrimaryInteractor == null) return;
        rotationDelta = _firearm.GetTriggerValue(); 
    }
}
