using Godot;
using NXRFirearm;
using NXR;


[GlobalClass]
public partial class FirearmParticlePlayer : GpuParticles3D
{
  [Export] private bool _restartOnFire = false;
  private Firearm _firearm;

  public override void _Ready()
  {
    if (Util.NodeIs(GetParent(), typeof(Firearm)))
    {
      _firearm = (Firearm)GetParent();
      _firearm.OnFire += OnFire;

    }
  }

  private void OnFire()
  {
    Emitting = true;

    if (_restartOnFire) Restart();
  }
  private void OnEject()
  {
    Emitting = true;

    if (_restartOnFire) Restart();
  }
}
