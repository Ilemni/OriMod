using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is using <see cref="ChargeDash"/>.
  /// </summary>
  // ReSharper disable once ClassNeverInstantiated.Global
  public sealed class ChargeDashProjectile : OriAbilityProjectile {
    public override int Id => AbilityId.ChargeDash;

    public override void SetDefaults() {
      base.SetDefaults();
      Projectile.width = 96;
      Projectile.height = 96;
      Projectile.damage = 40;
    }

    protected override void Behavior() {
      base.Behavior();
      // Size is stretched greatly based on velocity.
      Player player = aPlayer.Player;
      Vector2 vel = player.velocity;
      Projectile.width = (int)Utils.Clamp(Math.Abs(vel.X) * 1.5f, player.width * 1.5f, 96);
      Projectile.height = (int)Utils.Clamp(Math.Abs(vel.Y) * 1.5f, player.height * 1.5f, 96);
    }

    /// <summary>
    /// Ends <see cref="ChargeDash"/> if this hits the target NPC
    /// </summary>
    public override void OnHitNPC(NPC target, NPC.HitInfo modifiers, int damageDone) {
      ChargeDash cDash = abilities.chargeDash;
      if (cDash.NpcIsTarget(target)) {
        cDash.End();
      }
    }
  }
}
