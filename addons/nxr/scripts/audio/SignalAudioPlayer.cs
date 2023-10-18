using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;


[GlobalClass]
public partial class SignalAudioPlayer : AudioStreamPlayer3D
{
	[Export]
	private string _signal; 
	// Called when the node enters the scene tree for the first time.

	[Signal]
	public delegate void OnSigEventHandler(); 

	public override void _Ready()
	{
		if (GetParent().HasSignal(_signal)) { 
			Action signalAction = OnSignal; 
			GetParent().Connect(_signal, Callable.From(signalAction)); 
		}
	}

	private void OnSignal() { 
		
	}
}

