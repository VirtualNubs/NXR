using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;


[GlobalClass]
public partial class SignalSpawner: AudioStreamPlayer3D
{
	[Export]
	private string _signal; 
	// Called when the node enters the scene tree for the first time.


	public override void _Ready()
	{
		if (GetParent().HasSignal(_signal)) { 
			foreach (Dictionary item in GetParent().GetSignalList()) {
				if (item["name"].ToString() ==  _signal) {
					GD.Print(item["name"].ToString()); 
					Callable callable = Callable.From(() => OnSignal()); 
					Signal newSignal = new Signal(GetParent(), _signal); 
					GetParent().Connect(_signal, callable); 
				}
			}
		}
	}

	private void OnSignal() { 
		Play(); 
	}
}

