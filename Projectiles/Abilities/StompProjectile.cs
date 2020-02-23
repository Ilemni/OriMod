using System;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  internal class StompProjectile : AbilityProjectile {
    internal override int abilityID => AbilityID.Stomp;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 40;
      projectile.height = 56;
    }

    internal override void Behavior() {
      base.Behavior();
      projectile.height = Math.Max(56, (int)(oPlayer.player.velocity.Y * 2));
      projectile.position.Y += 10;
    }

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      var stomp = oPlayer.Abilities.stomp;
      if (target.life > 0 && stomp.InUse) {
        stomp.EndStomp();
      }
    }
  }
}
