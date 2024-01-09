using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class SignalAudioPlayer : AudioStreamPlayer3D
{
	[Export]
	private string _signal;
	// Called when the node enters the scene tree for the first time.


	public override void _Ready()
	{
		if (!GetParent().HasSignal(_signal)) return;

		foreach (Dictionary item in GetParent().GetSignalList())
		{
			if (item["name"].ToString() == _signal)
			{

				int argCount = item["args"].AsGodotArray().Count;

				if (argCount > 0)
				{
					Action<int[]> signalAction = OnSignalParams;
					Callable callable = Callable.From(signalAction);
					Signal newSignal = new Signal(GetParent(), _signal);
					GetParent().Connect(_signal, callable);
					return;
				}
				else
				{
					Callable callable = Callable.From(() => OnSignal());
					Signal newSignal = new Signal(GetParent(), _signal);
					GetParent().Connect(_signal, callable);
					return;
				}
			}
		}
	}


	private void OnSignal()
	{
		Action();
	}

	private void OnSignalParams(params int[] p)
	{
		Action();
	}

	private void Action()
	{
		Play();
	}
}

