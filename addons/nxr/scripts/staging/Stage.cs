using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Stage : Node3D
{
	[Export]
	private String _defaultPath = "";
	private String _path;

	[Export]
	private Node3D _loadingScene;
	private Godot.Collections.Array _progress = new();
	private Node _currentScene = null;
	private bool _queued = false;


	[ExportGroup("Delays")]
	[Export]
	private float _startDelay = 2f;
	private float _endDelay = 4f;


	[Signal]
	public delegate void TransitionQueuedEventHandler();
	[Signal]
	public delegate void ProgressUpdatedEventHandler(double progress);

	[Signal]
	public delegate void LoadedEventHandler(double progress);

	[Signal]
	public delegate void TransitionedEventHandler(double progress);


	public override void _Ready()
	{
		QueueTransition();
	}

	public override void _Process(double delta)
	{

		var status = ResourceLoader.LoadThreadedGetStatus(_path, _progress);

		if (!_queued) return;

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
				EmitSignal("Loaded");
				Transition();
				break;
		}
	}

	private async void Transition()
	{

		// set queued to false to stop updating the threaded status 
		_queued = false;

		EmitSignal("Transitioned");

		Resource scene = ResourceLoader.LoadThreadedGet(_path);
		PackedScene packed = (PackedScene)scene;
		Node newScene = packed.Instantiate();

		// optional delay for waiting for effects 
		await ToSignal(GetTree().CreateTimer(_startDelay), "timeout");

		// hide loading scene if available
		if (IsInstanceValid(_loadingScene)) _loadingScene.Hide();

		// add the new scene
		GetParent().AddChild(newScene);
		_currentScene = newScene;
	}
	public async void QueueTransition(string newPath = "")
	{
		// emit first to give time to react 
		EmitSignal("TransitionQueued");

		// show our loading scene if available
		if (IsInstanceValid(_loadingScene)) _loadingScene.Show();

		// remove current scene 
		if (IsInstanceValid(_currentScene))
		{
			_currentScene.GetParent().RemoveChild(_currentScene);
			_currentScene.QueueFree();
		}

		//wait for a couple seconds to show our delicious loading progress
		await ToSignal(GetTree().CreateTimer(_endDelay), "timeout");

		// set path to new path parameter if provided
		if (newPath != "")
		{
			_path = newPath;
		} else{ 
			_path = _defaultPath; 
		}

		// return if no path 
		if (_path == null) return;

		// do our threaded request 
		ResourceLoader.LoadThreadedRequest(_path, useSubThreads: false, cacheMode: ResourceLoader.CacheMode.Reuse);

		// set queued true to update the thread status in _process
		_queued = true;
	}

}
