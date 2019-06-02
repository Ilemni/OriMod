using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace OriMod.Projectiles {
	public class StompHitbox : ModProjectile {
    public OriPlayer Owner() {
      return Main.player[projectile.owner].GetModPlayer<OriPlayer>();
    }

    public override void SetStaticDefaults() { }

		public override void SetDefaults() {
			projectile.width = 100;
			projectile.height = 100;
			projectile.timeLeft = 40;
      projectile.penetrate = 999;
      projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
      projectile.friendly = true;
    }
    public override void AI() {
      projectile.Center = Main.player[projectile.owner].Center;
      switch (Owner.Abilities.stomp.State) {
        case Ability.States.Starting:
        case Ability.States.Active:
          projectile.width = 150;
          projectile.position.Y += 10;
          break;
        case Ability.States.Ending:
          break;
        default:
          projectile.Kill();
          break;
      }
      if (!Owner().movementHandler.stomp.IsState(Ability.States.Starting, Ability.States.Active)) { }
    }
    public override bool ShouldUpdatePosition() {
      return false;
    }
    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      if (Main.rand.Next(5) == 1) {
        crit = true;
      }
    }
  }
}