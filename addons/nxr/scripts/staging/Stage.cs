using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Stage : Node3D
{
	[Export] private String _defaultPath = "";
	[Export] private PackedScene _loadingScene;
	[Export] private bool _autoTransition = true;  


	[ExportGroup("Delays")]
	[Export] private float _startDelay = 2f;
	[Export] private float _endDelay = 4f;


	public bool IsQueued { get; private set; }


	private String _path;
	private Godot.Collections.Array _progress = new();
	private Node _currentScene = null;
	private Node _loadingSceneInstance = null; 


	[Signal] public delegate void TransitionQueuedEventHandler();
	[Signal] public delegate void ProgressUpdatedEventHandler(double progress);
	[Signal] public delegate void LoadedEventHandler(double progress);
	[Signal] public delegate void TransitionedEventHandler(double progress);

	[Signal] public delegate void PeerLoadedEventHandler(int id);	
	[Signal] public delegate void ServerLoadedEventHandler();	
	

	public override void _Ready()
	{
		QueueTransition();
		
	}


	public override void _Process(double delta)
	{

		if (!IsQueued) return; 


		var status = ResourceLoader.LoadThreadedGetStatus(_path, _progress);
		switch (status) { 
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


	private void SpawnLoadingScene() { 
		Node scene = _loadingScene.Instantiate(); 
		AddChild(scene); 
		_loadingSceneInstance = scene; 
	}


	public async void QueueTransition(string newPath = "")
	{

		// emit first to give time to react 
		EmitSignal("TransitionQueued");

		// spawn our loading scenes if available
		if (_loadingScene != null) SpawnLoadingScene();

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
		ResourceLoader.LoadThreadedRequest(_path);

		// set queued true to update the thread status in _process
		IsQueued = true;


		var status = ResourceLoader.LoadThreadedGetStatus(_path);
		if (status != ResourceLoader.ThreadLoadStatus.InProgress) { 
			GetTree().Quit(1); 
		}
	}


	private async void Transition()
	{
		// set queued to false to stop updating the threaded status 
		IsQueued = false;

		EmitSignal("Transitioned");

		Resource scene = ResourceLoader.LoadThreadedGet(_path);
		PackedScene packed = (PackedScene)scene;
		Node newScene = packed.Instantiate();

		// optional delay for waiting for effects 
		await ToSignal(GetTree().CreateTimer(_startDelay), "timeout");

		// delete loading scene if available
		if (IsInstanceValid(_loadingSceneInstance)) _loadingSceneInstance.QueueFree(); 

		// add the new scene
		GetParent().AddChild(newScene);
		_currentScene = newScene;
	}
}
