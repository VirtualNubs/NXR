using System.Runtime.Serialization.Formatters;
using Godot;
using NXR;
using NXRFirearm;


[Tool]
[GlobalClass]
public partial class FirearmTrigger : FirearmMovable
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
        RunTool(); 

        if (Engine.IsEditorHint()) return; 
         
        StartToEnd(_firearm.GetTriggerValue());
    }
}
