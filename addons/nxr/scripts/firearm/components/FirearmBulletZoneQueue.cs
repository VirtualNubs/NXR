using Godot;
using NXR;
using System.Collections.Generic;
using System.Linq;

namespace NXRFirearm;


/// <summary>
/// This class is used when using Bullet.cs for queing the next bullet to be shot
/// </summary>

[GlobalClass]
public partial class FirearmBulletZoneQueue : Node3D
{
	[Export] private Firearm _firearm;
	private List<FirearmBulletZone> _bulletZones = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		
		foreach (Node3D child in GetChildren())
		{
			if (Util.NodeIs(child, typeof(FirearmBulletZone))) _bulletZones.Add((FirearmBulletZone)child);
		}

		_firearm = FirearmUtil.GetFirearmFromParentOrOwner(this); 

		if (_firearm != null)
		{
			_firearm.OnFire += OnFire;
		}
	}

	public override void _Process(double delta)
	{
		if (GetSorted().Count > 0)
		{
			if (GetSorted().Last().Bullet != null)
			{
				_firearm.Chambered = !GetSorted().Last().Bullet.Spent;
			}
		}
	}

	private void OnFire()
	{
		if (GetSorted().Count > 0 && GetSorted().Last() != null)
		{
			GetSorted().Last().Bullet.Spent = true;
		}
	}

	public void EjectAll(Vector3 velocity, Vector3 torque, bool onlyEmpty=false) { 
		foreach (FirearmBulletZone zone in GetSorted()) { 
			if (onlyEmpty && zone.Bullet != null && zone.Bullet.Spent) { 
				zone.Eject(velocity, torque); 
			} 

			if (!onlyEmpty) { 
				zone.Eject(velocity, torque); 
			}
		}
	}

	private List<FirearmBulletZone> GetSorted()
	{
		if (_bulletZones.Count <= 0) return _bulletZones;
		
		List<FirearmBulletZone> list = new(); 

		foreach (Node3D child in GetChildren())
		{
			if (Util.NodeIs(child, typeof(FirearmBulletZone))) list.Add((FirearmBulletZone)child);
		}
		
		return list.OrderBy(x => x.Bullet != null && x.Bullet.Spent == false).ToList();
	}
}
