using Godot;
using NXR;
using System;

public partial class Pointer : RayCast3D
{

	[Export]
	private Controller _controller; 
	[Export]
	private float _velocityStrength = 1.0f; 
	private bool _hitting_gui = false; 
	private bool _is_activating_gui = false; 
	private Node3D _old_raycast_collider = null; 
	private Vector2 _oldViewportPoint; 
	private Node3D _raycast_colider = null; 
	private float _ws = 1.0f;


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Node3D _raycast_colider = (Node3D)GetCollider(); 

		if (_old_raycast_collider != null && _raycast_colider != _old_raycast_collider) { 
			ReleaseMouse(); 
		}

		if (_raycast_colider != null) { 
			TrySendInputToGUI(_raycast_colider); 

			Vector3 target = Vector3.Zero; 
			target.Z = -(GlobalPosition.DistanceTo(GetCollisionPoint()) + 0.05f); 
			TargetPosition = target; 

		} else { 

			Vector3 target = Vector3.Zero; 
			target.Z = -(2); 
			TargetPosition = target; 
			_hitting_gui = false; 
		}

		if (GetNode("BezierCurve3D") != null) { 
			BezierCurve3D curve = (BezierCurve3D)GetNode("BezierCurve3D"); 
			
			curve.EndPoint = TargetPosition; 
			curve.MidPoint.Z = TargetPosition.Z / 2; 
			curve.MidPoint.X = Mathf.Lerp(curve.MidPoint.X, 0, (float)delta); 
			curve.MidPoint.Y = Mathf.Lerp(curve.MidPoint.Y, 0, (float)delta); 
		} 

		if (_hitting_gui) { 
			Visible = true; 
		} else { 
			Visible = false; 
		}
	}

	private void TrySendInputToGUI(Node3D collider) { 
		if (collider.GetChildCount() <= 0) { 
			_hitting_gui = false; 
			return; 
		}



		if (!Util.NodeIs(collider.GetChild(0), typeof(SubViewport))) return;

		SubViewport vp = (SubViewport)collider.GetChild(0); 
		_hitting_gui = true; 

		CollisionShape3D shape = (CollisionShape3D)collider.GetChild(1); 
		Vector3 shapeSize = (Vector3)shape.Shape.Get("size");  
		Vector3 localPoint = collider.ToLocal(GetCollisionPoint()); 
		localPoint /= shapeSize; 
		localPoint += new Vector3(0.5f, -0.5f, 0f); 

		Vector2 viewportPoint = new Vector2(localPoint.X, -localPoint.Y) * new Vector2(vp.Size.X, vp.Size.Y); 

		InputEventMouseMotion eventMotion = new InputEventMouseMotion();
		eventMotion.Position = viewportPoint; 
		vp.PushInput(eventMotion); 

		bool desiredActivateGUI = _controller.GetFloat("trigger") > 0;

		if (desiredActivateGUI != _is_activating_gui) { 
			InputEventMouseButton clickEvent = new InputEventMouseButton(); 

			clickEvent.Pressed = desiredActivateGUI; 
			clickEvent.ButtonIndex = MouseButton.Left; 
			clickEvent.Position = viewportPoint; 
			vp.PushInput(clickEvent); 
			_is_activating_gui = desiredActivateGUI; 
			_old_raycast_collider = collider; 
			_oldViewportPoint = viewportPoint; 
		}
	}

	private void ReleaseMouse() { 
		SubViewport vp = (SubViewport)_old_raycast_collider.GetChild(0); 
		InputEventMouseButton clickEvent = new InputEventMouseButton(); 
		clickEvent.ButtonIndex = MouseButton.Left;
		clickEvent.Position = _oldViewportPoint; 
		vp.PushInput(clickEvent); 
		_old_raycast_collider = null; 
		_is_activating_gui = false; 
	}
}
