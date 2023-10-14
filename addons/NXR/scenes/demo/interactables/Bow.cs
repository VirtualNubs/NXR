using Godot;

using NXRInteractable; 

public partial class Bow : Interactable
{
    [Export]
    Interactable _stringHandle = null;

    [Export]
    BezierCurve3D _string = null;

    Transform3D _stringHandleInitXform;

    public override void _Ready()
    {
        _stringHandleInitXform = _stringHandle.Transform; 
    }

    public override void _PhysicsProcess(double delta)

    {

        base._PhysicsProcess(delta);

        float leng = (_stringHandleInitXform.Origin - _stringHandle.Position).Length(); 
        if (_stringHandle.IsGrabbed())
        {
        }
        else
        {
            _stringHandle.Position = _stringHandle.Position.Lerp(_stringHandleInitXform.Origin, (float)delta * 50.0f); 
        }

        Node3D parent = (Node3D)_string.GetParent();

        _string.MidPoint = _string.ToLocal(_stringHandle.GlobalPosition) ; 
    }

}
