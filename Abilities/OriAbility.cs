using AnimLib.Abilities;

namespace OriMod.Abilities;

public abstract class OriAbility : Ability<OriAbilityManager> {

  protected OriPlayer oPlayer => _oPlayer ??= abilities.oPlayer;
  protected OriInput input => _input ??= oPlayer.input;
  private OriPlayer _oPlayer;
  private OriInput _input;

  public override bool CanUse => base.CanUse && !player.shimmering && !player.frozen && !player.stoned && !player.webbed;

  protected bool OnWall => oPlayer.OnWall;
  protected bool IsGrounded => oPlayer.IsGrounded;
  protected void RestoreAirJumps() => oPlayer.RestoreAirJumps();

  protected void PlaySound(string path, float volume = 1, float pitch = 0)
    => oPlayer.PlaySound(path, volume, pitch);

  protected void PlayLocalSound(string path, float volume = 1, float pitch = 0)
    => oPlayer.PlayLocalSound(path, volume, pitch);
}
