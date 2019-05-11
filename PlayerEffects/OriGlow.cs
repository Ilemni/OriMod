using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.PlayerEffects {
  public class OriGlowRight : ModDust {
    public override void OnSpawn(Dust dust) {
      dust.noGravity = true;
      dust.frame = new Rectangle(0, 0, 104, 76);
      dust.alpha = 155;
    }

    public override bool Update(Dust dust) {
      dust.velocity = new Vector2(0, 0);
      dust.rotation = 0;
      dust.alpha += 4;
      if (dust.alpha >= 255)
      {
        dust.active = false;
      }
      return false;
      
    }
  }
}