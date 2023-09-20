using Godot;
using NXR;

public partial class Interactable : CharacterBody3D
{
    [Export]
    private DominantHand _dominantHand = DominantHand.Left;

    [Export]
    private Controller _leftController;
    [Export]
    private Controller _rightController;


    public Controller GetDominantController()
    {
        if (_dominantHand == DominantHand.Left) return _leftController;
        return _rightController; 
    }

    public Controller GetSecondaryController()
    {
        if (_dominantHand == DominantHand.Left) return _rightController;
        return _leftController;
    }

    public Vector2 GetDominantJoyAxis()
    {
        return GetDominantController().GetVector2("primary"); 
    }

    public Vector2 GetSecondaryJoyAxis()
    {
        return GetSecondaryController().GetVector2("primary");
    }

}
