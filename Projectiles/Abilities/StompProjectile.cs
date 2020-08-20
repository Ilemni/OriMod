using System;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  public sealed class StompProjectile : AbilityProjectile {
    public override byte abilityID => AbilityID.Stomp;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 40;
      projectile.height = 56;
      projectile.damage = 9 + (int)OriWorld.GlobalUpgrade * 9;
    }

    public override void Behavior() {
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
