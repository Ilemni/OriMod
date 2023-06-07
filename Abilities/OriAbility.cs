using AnimLib.Abilities;

namespace OriMod.Abilities;

public abstract class OriAbility : Ability<OriAbilityManager> {
  public override void Initialize() {
    base.Initialize();
    oPlayer = abilities.oPlayer;
    input = oPlayer.input;
  }
  
  protected OriPlayer oPlayer { get; private set; }
  protected OriInput input { get; private set; }

  protected bool OnWall => oPlayer.OnWall;
  protected bool IsGrounded => oPlayer.IsGrounded;

  protected void PlaySound(string path, float volume = 1, float pitch = 0)
    => oPlayer.PlaySound(path, volume, pitch);

    protected void PlayLocalSound(string path, float volume = 1, float pitch = 0)
        => oPlayer.PlayLocalSound(path, volume, pitch);
}
