using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace OriMod.Projectiles {
	public class OriDrown : ModProjectile {
    public OriPlayer Owner() {
      return Main.player[projectile.owner].GetModPlayer<OriPlayer>();
    }
    public override void SetStaticDefaults() { }
		public override void SetDefaults() {
			projectile.width = 100;
			projectile.height = 120;
			projectile.timeLeft = 800;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
      projectile.friendly = true;
    }
    public override void AI() { }
    public override bool ShouldUpdatePosition() {
      return false;
    }
    public override bool? CanCutTiles() {
      return false;
    }
  }
}