using System;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  internal class ChargeJumpProjectile : AbilityProjectile {
    internal override int abilityID => AbilityID.ChargeJump;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 96;
      projectile.height = 96;
    }

    internal override void CheckAbilityActive() {
      if (oPlayer.cJump.InUse || oPlayer.wCJump.InUse) {
        projectile.timeLeft = 2;
      }
    }

    internal override void Behavior() {
      base.Behavior();
      projectile.width = (int)Utils.Clamp(Math.Abs(oPlayer.player.velocity.X) * 2.5f, 96, 250);
      projectile.height = (int)Utils.Clamp(Math.Abs(oPlayer.player.velocity.Y) * 2.5f, 96, 250);
    }
  }
}
