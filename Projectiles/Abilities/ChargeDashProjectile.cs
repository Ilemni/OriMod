using System;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  internal class ChargeDashProjectile : AbilityProjectile {
    internal override int abilityID => AbilityID.ChargeDash;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 96;
      projectile.height = 96;
    }
    internal override void Behavior() {
      base.Behavior();
      projectile.width = (int)Utils.Clamp((Math.Abs(oPlayer.player.velocity.X) * 2.5f), 96, 250);
      projectile.height = (int)Utils.Clamp((Math.Abs(oPlayer.player.velocity.Y) * 2.5f), 96, 250);
    }
    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      if (target.whoAmI == oPlayer.cDash.NpcID) {
        oPlayer.cDash.End();
      }
    }
  }
}