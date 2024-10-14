using Godot;
using System;


namespace NXR;


public static class Util
{
	public static bool NodeIs(Node node, Type type)
	{

		if (node == null || type == null) return false;

		return node.GetType().IsAssignableTo(type);
	}

	public static Node GetNodeFromParentOrOwnerType(Node node, Type type) { 
		if (Util.NodeIs(node.GetParent(), type)) { 
			return node.GetParent(); 
		}

		if (Util.NodeIs(node.Owner, type)) {
			return  node.Owner; 
		}

		return null; 
	}

	public static async void Recenter(Node node)
	{
		float lifetime = Time.GetTicksMsec() / 1000.0f;
		float delay = 2f;

		if (lifetime > 2f) delay = 0;

		await node.ToSignal(node.GetTree().CreateTimer(delay), "timeout");
		XRServer.CenterOnHmd(XRServer.RotationMode.ResetButKeepTilt, true);
	}

		public static Basis BasisSlerped(Basis from, Basis to, float amount) { 
			Quaternion q1 = from.Orthonormalized().GetRotationQuaternion(); 
			Quaternion q2 = to.Orthonormalized().GetRotationQuaternion(); 
			Quaternion q3 = q1.Normalized().Slerp(q2.Normalized(), amount); 

			return new Basis(q3).Orthonormalized();
	}	

}
