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
      if (Owner().stomping) {
        projectile.width = 150;
      }
      projectile.Center = Main.player[projectile.owner].Center;
      if (Owner().stompHitboxTimer > 0) {
        projectile.Center = Main.player[projectile.owner].Center;
        projectile.position.Y += 10;
      }
      else {
        projectile.Kill();
      }
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