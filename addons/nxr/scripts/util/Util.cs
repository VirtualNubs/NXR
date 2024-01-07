using Godot;
using System;

namespace NXR; 

using Godot; 
public static class Util 
{
    public static bool NodeIs(Node node, Type type ) { 

        if (node == null || type ==  null) return false; 
        
        return node.GetType().IsAssignableTo(type); 
    }

    public static async void Recenter(Node node) { 
		float lifetime = Time.GetTicksMsec() / 1000.0f; 
		float delay = 2f; 

		if (lifetime > 2f) delay = 0; 

		await node.ToSignal(node.GetTree().CreateTimer(delay), "timeout"); 
		XRServer.CenterOnHmd(XRServer.RotationMode.ResetButKeepTilt, true); 
	}
}
