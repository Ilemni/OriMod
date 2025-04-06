using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities;

/// <summary>
/// Projectile hitbox for when the player is using <see cref="ChargeDash"/>.
/// </summary>
public sealed class ChargeDashProjectile : OriAbilityProjectile<ChargeDash> {
  public override void SetDefaults() {
    base.SetDefaults();
    Projectile.width = 96;
    Projectile.height = 96;
    Projectile.damage = 40;
  }

  protected override void Behavior() {
    base.Behavior();
    // Size is stretched greatly based on velocity.
    Player player = Player;
    Vector2 vel = player.velocity * 1.5f;
    Vector2 playerSize = player.Size * 1.5f;
    Projectile.width = (int)Math.Clamp(Math.Abs(vel.X), playerSize.X, 96);
    Projectile.height = (int)Math.Clamp(Math.Abs(vel.Y), playerSize.X, 96);
  }

  /// <summary>
  /// Ends <see cref="ChargeDash"/> if this hits the target NPC
  /// </summary>
  public override void OnHitNPC(NPC target, NPC.HitInfo modifiers, int damageDone) {
    if (Ability.Active && Ability.NpcIsTarget(target)) {
      Ability.EndByNpcContact(target);
    }
  }
}
