using OriMod.Abilities;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is using <see cref="Launch"/>.
  /// <para>Ending stomp spawns a <see cref="StompEnd"/> projectile to deal damage.</para>
  /// </summary>
  public sealed class LaunchProjectile : AbilityProjectile {
    public override byte Id => AbilityId.Launch;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 56;
      projectile.height = 56;
    }

    protected override void CheckAbilityActive() {
      if (ability.Active) {
        projectile.timeLeft = 10;
      }
    }
  }
}

