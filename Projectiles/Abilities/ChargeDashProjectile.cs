using System;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is using <see cref="ChargeDash"/>.
  /// </summary>
  public sealed class ChargeDashProjectile : AbilityProjectile {
    public override byte abilityID => AbilityID.ChargeDash;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 96;
      projectile.height = 96;
      projectile.damage = 12 + (int)OriWorld.GlobalUpgrade * 9;
    }

    public override void Behavior() {
      base.Behavior();
      // Size is stretched greatly based on velocity.
      var vel = oPlayer.player.velocity;
      projectile.width = (int)Utils.Clamp(Math.Abs(vel.X) * 2.5f, 96, 250);
      projectile.height = (int)Utils.Clamp(Math.Abs(vel.Y) * 2.5f, 96, 250);
    }

    /// <summary>
    /// Ends <see cref="ChargeDash"/> if this hits the target NPC
    /// </summary>
    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      var cDash = oPlayer.abilities.chargeDash;
      if (cDash.NpcIsTarget(target)) {
        cDash.End();
      }
    }
  }
}
