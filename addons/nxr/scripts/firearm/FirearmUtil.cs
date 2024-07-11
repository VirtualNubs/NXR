using Godot;
using NXR;
using System;


namespace NXRFirearm; 

public partial class FirearmUtil : Node
{
	public static Firearm GetFirearmFromParentOrOwner(Node node) { 
		if (Util.NodeIs(node.GetParent(), typeof(Firearm))) { 
			return (Firearm)node.GetParent(); 
		}

		if (Util.NodeIs(node.Owner, typeof(Firearm))) {
			return  (Firearm)node.Owner; 
		}

		return null; 
	}
}
