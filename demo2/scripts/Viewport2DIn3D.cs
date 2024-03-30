using System;
using System.Reflection.Metadata.Ecma335;
using Godot;

[Tool]
public partial class Viewport2DIn3D : Node3D
{
	[Export]
	public PackedScene SubScene
	{
		get => _subScene;
		set
		{
			_subScene = value;
			_dirty |= Dirty.SubScene;

			if (!_isReady) return;
			UpdateRender();
		}
	}

	[ExportGroup("Flag Controls")]
	[Export]
	public bool TriggerRedraw
	{
		set => _dirty |= Dirty.Redraw;
		get => false;
	}
	[Export]
	public bool TriggerRematerial
	{
		set => _dirty |= Dirty.Material;
		get => false;
	}
	[Export]
	public bool TriggerResurface
	{
		set => _dirty |= Dirty.Surface;
		get => false;
	}
	[Export]
	public bool TriggerRealbedo
	{
		set => _dirty |= Dirty.Albedo;
		get => false;
	}
	[Export]
	public bool TriggerResubscene
	{
		set => _dirty |= Dirty.SubScene;
		get => false;
	}

	[Flags]
	private enum Dirty
	{
		None = 0,
		Material = 1,
		SubScene = 2,
		Size = 4,
		Albedo = 8,
		Update = 16,
		Transparency = 32,
		AlphaScissor = 64,
		Unshaded = 128,
		Filtered = 256,
		Surface = 512,
		Redraw = 1024,
		All = 2047
	}

	private PackedScene _subScene;
	private double _timeSinceUpdate = 0;
	private Control _subSceneInstance;
	private Dirty _dirty = Dirty.All;
	private bool _isReady = false;
	private StandardMaterial3D _screenMaterial;
	private SubViewport _subViewport;
	private MeshInstance3D _screen;

	public override void _Ready()
	{
		_isReady = true;

		_subViewport = GetNode<SubViewport>("%SubViewport");
		_screen = GetNode<MeshInstance3D>("%Screen");

		UpdateRender();
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			_timeSinceUpdate += delta;
			if (_timeSinceUpdate >= 1.0)
			{
				_timeSinceUpdate = 0;

				_dirty |= Dirty.Material;
				UpdateRender();
			}
		}
		else
		{
			float frameTime = 1 / 30;
			_timeSinceUpdate += delta;
			if (_timeSinceUpdate > frameTime)
			{
				_timeSinceUpdate = 0;

				_subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
			}
		}
	}

	public SubViewport GetSubViewport() => _subViewport;

	private void UpdateRender()
	{
		if (Engine.IsEditorHint())
		{
			_subViewport = GetNode<SubViewport>("%SubViewport");
			_screen = GetNode<MeshInstance3D>("%Screen");
		}

		if (_dirty.HasFlag(Dirty.Material))
		{
			_dirty &= ~Dirty.Material;

			_screenMaterial = new StandardMaterial3D
			{
				CullMode = BaseMaterial3D.CullModeEnum.Disabled
			};

			_dirty |= Dirty.Albedo | Dirty.Surface;
		}

		if (_dirty.HasFlag(Dirty.SubScene))
		{
			_dirty &= ~Dirty.SubScene;

			if (_subSceneInstance is not null && IsInstanceValid(_subSceneInstance))
			{
				_subViewport.RemoveChild(_subSceneInstance);
				_subSceneInstance.QueueFree();
			}

			if (_subScene is not null)
			{
				_subSceneInstance = _subScene.Instantiate<Control>();
				_subViewport.AddChild(_subSceneInstance);
			}

			_dirty |= Dirty.Redraw;
		}

		if (_dirty.HasFlag(Dirty.Albedo))
		{
			_dirty &= ~Dirty.Albedo;

			_screenMaterial.AlbedoTexture = _subViewport.GetTexture();
		}

		if (_dirty.HasFlag(Dirty.Surface))
		{
			_dirty &= ~Dirty.Surface;

			_screen.SetSurfaceOverrideMaterial(0, _screenMaterial);
		}

		if (_dirty.HasFlag(Dirty.Redraw))
		{
			_dirty &= ~Dirty.Redraw;

			if (Engine.IsEditorHint()) _subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
		}
	}
}
