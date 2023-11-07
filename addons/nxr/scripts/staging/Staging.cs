using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Staging : Node
{
	[Export]
	private String path;
	private Godot.Collections.Array _progress = new();
	private Node3D _currentLevel = null;

	[Signal]
	public delegate void ProgressUpdatedEventHandler(double progress);

	[Signal]
	public delegate void LoadedEventHandler(double progress);


	public override void _Process(double delta)
	{

		if (Input.IsActionJustPressed("ui_accept"))
		{
			QueueTransition();
		}

		var status = ResourceLoader.LoadThreadedGetStatus(path, _progress);

		switch (status)
		{
			case ResourceLoader.ThreadLoadStatus.Failed:
				GD.PrintErr("Error loading: Loading Failed");
				break;
			case ResourceLoader.ThreadLoadStatus.InvalidResource:
				GD.PrintErr("Error loading: Invalid Resource");
				break;
			case ResourceLoader.ThreadLoadStatus.InProgress:
				double progress = (double)_progress[0] * 100.0;
				EmitSignal("ProgressUpdated", progress);
				break;
			case ResourceLoader.ThreadLoadStatus.Loaded:
				Resource scene = ResourceLoader.LoadThreadedGet(path); 
				PackedScene packed = (PackedScene)scene; 
				
				break;
		}
	}

	void QueueTransition()
	{
		if (path == null) return;

		ResourceLoader.LoadThreadedRequest(path, useSubThreads: true, cacheMode: ResourceLoader.CacheMode.Reuse);
	}
}
