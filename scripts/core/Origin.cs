using Godot;
using System;

public partial class Origin : XROrigin3D
{
    public override void _Ready() {
        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled); 
        GetViewport().UseXR = true; 
    }
}
