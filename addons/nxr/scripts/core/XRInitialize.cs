using System;
using System.Linq;
using Godot;
using Godot.Collections;


[GlobalClass]
public partial class XRInitialize : Node
{
    public XRInterface CurrentInterface { get; set; }
    int _refreshRate = 72; 

    public override void _Ready() {
        GetViewport().UseXR = true; 
        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled); 

        CurrentInterface = XRServer.FindInterface("OpenXR"); 

        if(CurrentInterface == null) return;   


        CurrentInterface.Connect("pose_recentered", Callable.From(PoseRecenter));


        int reported_rate = (int)CurrentInterface.Call("get_display_refresh_rate");
        Godot.Collections.Array rates = (Godot.Collections.Array)CurrentInterface.Call("get_available_display_refresh_rates"); 

        if (rates.Count > 0) { 
            _refreshRate = (int)rates.Last(); 
        } else { 
            _refreshRate = reported_rate; 
        }
    
        GD.Print(
            string.Format(
                "Refresh rate is: {0}, setting tick rate to {1} ", 
                _refreshRate, _refreshRate
            )
        ); 

        Engine.PhysicsTicksPerSecond = _refreshRate;  
    }


    private void PoseRecenter() { 
        XRServer.CenterOnHmd(XRServer.RotationMode.ResetButKeepTilt, true); 
    }
}
