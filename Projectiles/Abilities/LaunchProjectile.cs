using OriMod.Abilities;

namespace OriMod.Projectiles.Abilities; 

/// <summary>
/// Projectile hitbox for when the player is using <see cref="Launch"/>.
/// <para>Ending stomp spawns a <see cref="StompEnd"/> projectile to deal damage.</para>
/// </summary>
public sealed class LaunchProjectile : OriAbilityProjectile {
  public override int Id => AbilityId.Launch;

  public override void SetDefaults() {
    base.SetDefaults();
    Projectile.width = 56;
    Projectile.height = 56;
  }

  protected override void CheckAbilityActive() {
    if (ability.Active) {
      Projectile.timeLeft = 10;
    }
  }
}
