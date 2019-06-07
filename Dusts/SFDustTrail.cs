using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  public class SFDustTrail : ModDust {
    protected int alphaRate = 1;
    public override void OnSpawn(Dust dust) {
      dust.alpha = 0;
      dust.noGravity = true;
    }
    public override bool Update(Dust dust) {
      dust.position += dust.velocity;
      dust.alpha += alphaRate;
      if (dust.alpha > 14) {
        dust.alpha = 255;
        dust.active = false;
      }
      else if (dust.alpha > 11) {
        dust.frame = new Rectangle(0, 40, 10, 10);
      }
      else if (dust.alpha > 8) {
        dust.frame = new Rectangle(0, 30, 10, 10);
      }
      else if (dust.alpha > 5) {
        dust.frame = new Rectangle(0, 20, 10, 10);
      }
      else if (dust.alpha > 2) {
        dust.frame = new Rectangle(0, 10, 10, 10);
      }
      return false;
    }
  }
}