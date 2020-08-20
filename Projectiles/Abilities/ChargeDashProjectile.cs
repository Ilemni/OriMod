using System;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  public sealed class ChargeDashProjectile : AbilityProjectile {
    public override byte abilityID => AbilityID.ChargeDash;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 96;
      projectile.height = 96;
    }

    public override void Behavior() {
      base.Behavior();
      var vel = oPlayer.player.velocity;
      projectile.width = (int)Utils.Clamp(Math.Abs(vel.X) * 2.5f, 96, 250);
      projectile.height = (int)Utils.Clamp(Math.Abs(vel.Y) * 2.5f, 96, 250);
    }

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      var cDash = oPlayer.Abilities.chargeDash;
      if (cDash.NpcIsTarget(target)) {
        cDash.End();
      }
    }
  }
}
