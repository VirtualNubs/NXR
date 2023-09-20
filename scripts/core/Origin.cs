using Godot;
using System;

public partial class Origin : XROrigin3D
{
    public override void _Ready() { 
        GetViewport().UseXR = true; 
    }
}
