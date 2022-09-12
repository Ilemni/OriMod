using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  /// <summary>
  /// Dust used as a trail for <see cref="Projectiles.Minions.SpiritFlame"/>.
  /// </summary>
  public class SpiritFlameDustTrail : ModDust {
    private int AlphaRate => 1;

    public override string Texture => @"OriMod/Dusts/SFDust1";

    public override void OnSpawn(Dust dust) {
      dust.alpha = 0;
      dust.noGravity = true;
    }

    public override bool Update(Dust dust) {
      dust.position += dust.velocity;
      dust.alpha += AlphaRate;
      if (dust.alpha > 14) {
        dust.alpha = 255;
        dust.active = false;
      }
      else if (dust.alpha > 11) {
        dust.frame.Y = 40;
      }
      else if (dust.alpha > 8) {
        dust.frame.Y = 30;
      }
      else if (dust.alpha > 5) {
        dust.frame.Y = 20;
      }
      else if (dust.alpha > 2) {
        dust.frame.Y = 10;
      }
      return false;
    }
  }
}
