using System;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is in the stomping animation. Used to end <see cref="Stomp"/>.
  /// <para>Ending stomp spawns a <see cref="StompEnd"/> projectile to deal damage.</para>
  /// </summary>
  // ReSharper disable once ClassNeverInstantiated.Global
  public sealed class StompProjectile : AbilityProjectile {
    public override byte Id => AbilityId.Stomp;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 40;
      projectile.height = 56;
    }

    protected override void Behavior() {
      base.Behavior();
      // Height is stretched based on velocity.
      projectile.height = Math.Max(56, (int)(oPlayer.player.velocity.Y * 2));
      projectile.position.Y += 10;
    }

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      Stomp stomp = oPlayer.abilities.stomp;
      if (target.life > 0 && stomp.InUse) {
        stomp.EndStomp();
      }
    }
  }
}
