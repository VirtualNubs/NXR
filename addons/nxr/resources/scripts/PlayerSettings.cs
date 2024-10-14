using Godot;
using System;
using NXRPlayer; 

[GlobalClass]
public partial class PlayerSettings : Resource
{
	
	[Export] public DominantHand DominantHand { get; set; }
	[Export] public RotationMode RotationMode { get; set; }

	public PlayerSettings() : this(0, 0) {}


	public PlayerSettings(DominantHand hand, RotationMode rotMode) { 
		DominantHand = hand; 
		RotationMode = rotMode;
	}


	public static void UpdateSettings(PlayerSettings settings) { 
		ResourceSaver.Save(settings, "user://" + settings.ResourceName + ".tres"); 
	}


	public static PlayerSettings LoadSettings(string resource_name="default_player_settings") { 
		String path = "user://" + resource_name + ".tres"; 
		bool exists = ResourceLoader.Exists(path);

		return (PlayerSettings)ResourceLoader.Load(path); 
	}


	public static bool SettingsExist(string resource_name) { 
		String path = "user://" + resource_name + ".tres"; 
		return ResourceLoader.Exists(path);
	}
}

