using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  public class AbilityRefreshedDust : ModDust {
    protected int alphaRate = 12;
    private int Speed = 3; 
    public override void OnSpawn(Dust dust) {
      dust.alpha = 0;
      dust.noGravity = true;
      dust.velocity = Vector2.UnitX.RotateRandom(2 * Math.PI) * Speed;
    }
    public override bool Update(Dust dust) {
      dust.position += dust.velocity;
      dust.alpha += alphaRate;
      if (dust.alpha > 255) {
        dust.alpha = 255;
        dust.active = false;
      }
      return false;
    }
  }
}