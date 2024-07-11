using Godot;
using NXR;
using System;

public partial class Pointer : RayCast3D
{

	#region Exported
	[Export] private Controller _controller; 
	[Export] private float _velocityStrength = 1.0f; 
	#endregion


	#region Private
	private bool _hittingGui = false; 
	private bool _isActivatingGui = false; 
	private Node3D _oldCollider = null; 
	private Vector2 _oldViewportPoint; 
	private Node3D _raycastColider = null; 
	private float _ws = 1.0f;
	# endregion



	public override void _Process(double delta)
	{
		Node3D collider = (Node3D)GetCollider(); 

		if (_oldCollider != null && collider != _oldCollider) { 
			ReleaseMouse(); 
		}

		if (collider != null) { 
			TrySendInputToGUI(collider); 

			Vector3 target = Vector3.Zero; 
			target.Z = -(GlobalPosition.DistanceTo(GetCollisionPoint()) + 0.05f); 
			TargetPosition = target; 

		} else { 
			Vector3 target = Vector3.Zero; 
			target.Z = -(2); 
			TargetPosition = target; 
			_hittingGui = false; 
		}

		if (GetNode("BezierCurve3D") != null) { 
			BezierCurve3D curve = (BezierCurve3D)GetNode("BezierCurve3D"); 
			
			curve.EndPoint = TargetPosition; 
			curve.MidPoint.Z = TargetPosition.Z / 2; 
			curve.MidPoint.X = Mathf.Lerp(curve.MidPoint.X, 0, (float)delta); 
			curve.MidPoint.Y = Mathf.Lerp(curve.MidPoint.Y, 0, (float)delta); 
		} 

		if (_hittingGui) { 
			Visible = true; 
		} else { 
			Visible = false; 
		}
	}

	private void TrySendInputToGUI(Node3D collider) { 
		if (collider.GetChildCount() <= 0) { 
			_hittingGui = false; 
			return; 
		}



		if (!Util.NodeIs(collider.GetChild(0), typeof(SubViewport))) return;

		SubViewport vp = (SubViewport)collider.GetChild(0); 
		_hittingGui = true; 

		CollisionShape3D shape = (CollisionShape3D)collider.GetChild(1); 
		Vector3 shapeSize = (Vector3)shape.Shape.Get("size");  
		Vector3 localPoint = collider.ToLocal(GetCollisionPoint()); 
		localPoint /= new Vector3(shapeSize.X, shapeSize.Y, shapeSize.Z); 
		localPoint += new Vector3(0.5f, -0.5f, 0f); 


		Vector2 viewportPoint = new Vector2(localPoint.X, -localPoint.Y) * new Vector2(vp.Size.X, vp.Size.Y); 

		InputEventMouseMotion eventMotion = new InputEventMouseMotion();
		eventMotion.Position = viewportPoint; 
		vp.PushInput(eventMotion); 

		bool desiredActivateGUI = _controller.GetFloat("trigger") > 0;

		if (desiredActivateGUI != _isActivatingGui) { 
			InputEventMouseButton clickEvent = new InputEventMouseButton(); 

			clickEvent.Pressed = desiredActivateGUI; 
			clickEvent.ButtonIndex = MouseButton.Left; 
			clickEvent.Position = viewportPoint; 
			vp.PushInput(clickEvent); 
			_isActivatingGui = desiredActivateGUI; 
			_oldCollider = collider; 
			_oldViewportPoint = viewportPoint; 
		}
	}


	private void ReleaseMouse() { 
		SubViewport vp = (SubViewport)_oldCollider.GetChild(0); 
		InputEventMouseButton clickEvent = new InputEventMouseButton(); 
		clickEvent.ButtonIndex = MouseButton.Left;
		clickEvent.Position = _oldViewportPoint; 
		vp.PushInput(clickEvent); 
		_oldCollider = null; 
		_isActivatingGui = false; 
	}
}
