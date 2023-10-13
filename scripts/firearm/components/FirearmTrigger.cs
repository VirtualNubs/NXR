using System.Runtime.Serialization.Formatters;
using Godot;
using NXR;
using NXRFirearm;


[Tool]
[GlobalClass]
public partial class FirearmTrigger : InputRotator
{

    private Firearm _firearm = null;

    public override void _Ready()
    {
        if (Util.NodeIs(GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        } else { 
            GD.PushWarning("No Firearm Found!"); 
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        
        if (_firearm == null) return;

        base._PhysicsProcess(delta);

        if (Engine.IsEditorHint()) return; 
        if (_firearm.PrimaryInteractor == null) return;

        if (Target == null) { 
            Target = (Node3D)this; 
        }

        rotationDelta = _firearm.GetTriggerValue(); 
    }
}
