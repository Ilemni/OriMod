using System;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is in the stomping animation. Used to end <see cref="Stomp"/>.
  /// <para>Ending stomp spawns a <see cref="StompEnd"/> projectile to deal damage.</para>
  /// </summary>
  public sealed class StompProjectile : AbilityProjectile {
    public override byte abilityID => AbilityID.Stomp;

    public static int Damage = 9 + (int)OriWorld.GlobalUpgrade * 9;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 40;
      projectile.height = 56;
      projectile.damage = Damage;
    }

    public override void Behavior() {
      base.Behavior();
      // Height is stretched based on velocity.
      projectile.height = Math.Max(56, (int)(oPlayer.player.velocity.Y * 2));
      projectile.position.Y += 10;
    }

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      var stomp = oPlayer.abilities.stomp;
      if (target.life > 0 && stomp.InUse) {
        stomp.EndStomp();
      }
    }
  }
}
