using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace OriMod.Projectiles {
	public class StompHitbox : ModProjectile {
    private OriPlayer _owner;
    public OriPlayer Owner {
      get {
        if (_owner == null) {
          _owner = Main.player[projectile.owner].GetModPlayer<OriPlayer>();
        }
        return _owner;
      }
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
      if (Owner.stomp.InUse) {
        switch (Owner.stomp.State) {
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
      }
      else if (Owner.cDash.InUse) {
        projectile.width = 80;
        projectile.height = 80;
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