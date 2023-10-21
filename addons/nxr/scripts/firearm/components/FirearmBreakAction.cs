using Godot;
using NXR;

namespace NXRFirearm; 

[Tool]
[GlobalClass]
public partial class FirearmBreakAction : FirearmMovable
{

	[Export]
	private Firearm _firearm; 
	
    public override void _Ready()
    {

        if (Util.NodeIs((Node)GetParent(), typeof(Firearm)))
        {
            _firearm = (Firearm)GetParent();
        }
    }

    public override void _Process(double delta) { 
		
		if (Engine.IsEditorHint()) { 
			RunTool();
		}

		if (_firearm == null) return; 

		if (_firearm.GetPrimaryInteractor() != null && _firearm.GetPrimaryInteractor().Controller.ButtonOneShot("ax_button")) { 
			Open(); 
		}

		if (IsOpen() && GetCloseInput()) { 
			Close(); 
		}
	}

	private bool IsClosed() { 
		return StartXform.IsEqualApprox(StartXform); 
	}
	
	private bool IsOpen() { 
		return EndXform.IsEqualApprox(EndXform); 
	}


	public void Open() { 
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Target, "transform", EndXform, 0.1f); 
	}

	public void Close() { 
		Tween tween = GetTree().CreateTween(); 
		tween.TweenProperty(Target, "transform", StartXform, 0.1f); 
	}
	
	private bool GetCloseInput() { 
		if (_firearm.GetPrimaryInteractor() == null) return false; 
		
		Controller controller = _firearm.GetPrimaryInteractor().Controller; 

		return controller.LocalVelMatches(GlobalTransform.Basis.Y, 30); 
	}
}
