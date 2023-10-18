using Godot;
using System;

namespace NXR; 
public static class Util 
{
    public static bool NodeIs(Node node, Type type ) { 

        if (node == null || type ==  null) return false; 
        
        return node.GetType().IsAssignableTo(type); 
    }
}
