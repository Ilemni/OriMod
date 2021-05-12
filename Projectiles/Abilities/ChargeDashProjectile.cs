using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is using <see cref="ChargeDash"/>.
  /// </summary>
  // ReSharper disable once ClassNeverInstantiated.Global
  public sealed class ChargeDashProjectile : AbilityProjectile {
    public override byte Id => AbilityId.ChargeDash;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 96;
      projectile.height = 96;
      projectile.damage = 40;
    }

    protected override void Behavior() {
      base.Behavior();
      // Size is stretched greatly based on velocity.
      Player player = oPlayer.player;
      Vector2 vel = player.velocity;
      projectile.width = (int)Utils.Clamp(Math.Abs(vel.X) * 1.5f, player.width * 1.5f, 96);
      projectile.height = (int)Utils.Clamp(Math.Abs(vel.Y) * 1.5f, player.height * 1.5f, 96);
    }

    /// <summary>
    /// Ends <see cref="ChargeDash"/> if this hits the target NPC
    /// </summary>
    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      ChargeDash cDash = oPlayer.abilities.chargeDash;
      if (cDash.NpcIsTarget(target)) {
        cDash.End();
      }
    }
  }
}
