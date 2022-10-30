using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for when the player is using <see cref="ChargeJump"/> and <see cref="WallChargeJump"/>.
  /// </summary>
  public sealed class ChargeJumpProjectile : OriAbilityProjectile {
    /// <summary>
    /// This is not used, as this <see cref="OriAbilityProjectile"/> is used by both <see cref="ChargeJump"/> and <see cref="WallChargeJump"/>.
    /// </summary>
    public override int Id => AbilityId.ChargeJump;

    public override void SetDefaults() {
      base.SetDefaults();
      Projectile.width = 96;
      Projectile.height = 96;
    }

    protected override void CheckAbilityActive() {
      if (abilities.chargeJump || abilities.wallChargeJump) {
        Projectile.timeLeft = 2;
      }
    }

    protected override void Behavior() {
      base.Behavior();
      // Stretch projectile size based on velocity
      Vector2 vel = aPlayer.Player.velocity;
      Projectile.width = (int)Utils.Clamp(Math.Abs(vel.X) * 2.5f, 96, 250);
      Projectile.height = (int)Utils.Clamp(Math.Abs(vel.Y) * 2.5f, 96, 250);
    }
  }
}
