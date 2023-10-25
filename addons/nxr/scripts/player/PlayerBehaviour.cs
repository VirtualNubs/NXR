using Godot;
using System;

namespace NXRPlayer; 
public partial class PlayerBehaviour : Node
{

    protected Player _player;

    public override void _Ready()
    {
        if (GetParent().GetClass() == "CharacterBody3D")
        {
            _player = (Player)GetParent(); 
        } else
        {
            GD.PushWarning("No player body found!"); 
        }
    }
}
