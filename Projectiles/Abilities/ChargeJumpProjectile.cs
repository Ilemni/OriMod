using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities;

/// <summary>
/// Projectile hitbox for when the player is using <see cref="ChargeJump"/> and <see cref="WallChargeJump"/>.
/// </summary>
public sealed class ChargeJumpProjectile : OriAbilityProjectile<ChargeJump> {
  public override void SetDefaults() {
    base.SetDefaults();
    Projectile.width = 96;
    Projectile.height = 96;
  }

  protected override void CheckAbilityActive() {
    if (OriPlayer.ActiveState is ChargeJump or WallChargeJump) {
      Projectile.timeLeft = 2;
    }
  }

  protected override void Behavior() {
    base.Behavior();
    // Stretch projectile size based on velocity
    Point vel = (Player.velocity * 2.5f).ToPoint();
    Projectile.width = Math.Clamp(Math.Abs(vel.X), 96, 250);
    Projectile.height = Math.Clamp(Math.Abs(vel.Y), 96, 250);
  }
}
