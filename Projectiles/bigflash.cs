using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace OriMod.Projectiles {
  public class bigflash : ModProjectile {
    public override void SetStaticDefaults() { }

    public override void SetDefaults() {
      projectile.width = 512;
      projectile.height = 512;
      projectile.timeLeft = 120;
      projectile.magic = true;
      projectile.tileCollide = false;
      projectile.ignoreWater = true;
      projectile.friendly = true;
      projectile.scale = 0.01f;
      projectile.damage = 0;
    }
    public override bool ShouldUpdatePosition() {
      return false;
    }
    public override bool? CanCutTiles() {
      return false;
    }
  }
}