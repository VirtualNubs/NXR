using Godot;
using System;
using NXRPlayer; 

[GlobalClass]
public partial class PlayerSettings : Resource
{
	[Export]
	public DominantHand DominantHand { get; set; }

	[Export]
	public RotationMode RotationMode { get; set; }


	public PlayerSettings() : this(0, 0) {}

	public PlayerSettings(DominantHand hand, RotationMode rotMode) { 
		DominantHand = hand; 
		RotationMode = rotMode; 
	}

	public void UpdateSettings(PlayerSettings settings) { 
		bool exists = ResourceLoader.Exists(settings.ResourcePath); 


		if (exists) {
			ResourceSaver.Save(settings, settings.ResourcePath); 
		}
	}
}

