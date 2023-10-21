using Godot;
using NXRInteractable;
using System;
using System.ComponentModel;

namespace NXRFirearm; 

[Tool]
public partial class FirearmMovable : Interactable
{
	[ExportGroup("Tool Settings")]
	[Export]
	public Node3D Target; 

	[Export]
	private bool _setStartXform = false; 
	[Export]
	private bool _setEndXform = false; 

	[Export]
	public bool _goStart  = false; 
	[Export]
	public bool _goEnd = false; 
	
	[ExportGroup("Transforms")]
	[Export]
	public Transform3D StartXform; 
	[Export]
	public Transform3D EndXform; 
	
	public void RunTool()
	{
		if(Target == null) { 
			Target = this; 
		}

		if (_setStartXform) { 
			StartXform = Target.Transform.Orthonormalized(); 
			_setStartXform = false;
		}
		if (_setEndXform) { 
			EndXform = Target.Transform.Orthonormalized(); 
			_setEndXform = false;
		}

		if(_goStart) { 
			Target.Transform = StartXform.Orthonormalized(); 
			_goStart = false; 
		}

		if (_goEnd) { 
			Target.Transform = EndXform.Orthonormalized();
			_goEnd = false; 
		}
	}

	public void GoStart() { 
		Target.Transform = StartXform; 
	}

	public void GoEnd() { 
		Target.Transform = EndXform; 
	}
}
