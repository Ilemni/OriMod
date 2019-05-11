using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace OriMod.Projectiles {
	public class BashHitbox : ModProjectile {
    public OriPlayer Owner() {
      return Main.player[projectile.owner].GetModPlayer<OriPlayer>();
    }

    public override void SetStaticDefaults() { }

		public override void SetDefaults() {
			projectile.width = 175;
			projectile.height = 175;
			projectile.timeLeft = 40;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
      projectile.friendly = true;
    }
    public override void AI() {
      projectile.Center = Main.player[projectile.owner].Center;
      if (Owner().bashActivate > 0 || Owner().bashActiveTimer >= 6) {
        projectile.Center = Main.player[projectile.owner].Center;
      }
      else {
        Owner().PlayNewSound("Ori/Bash/bashNoTargetB");
        projectile.Kill();
      }
    }
    public override bool ShouldUpdatePosition() {
      return false;
    }
    public override bool? CanCutTiles() {
      return false;
    }
    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      if (Owner().bashActivate > 0 && Owner().bashActiveTimer == 0 && Owner().bashActive == false) {
        Owner().tempInvincibility = true;
        Owner().immuneTimer = 15;
        Owner().BashEffects(target);
      }
    }
  }
}