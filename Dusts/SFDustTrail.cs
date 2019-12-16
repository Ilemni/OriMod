using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  public class SFDustTrail : ModDust {
    private int AlphaRate => 1;

    public override bool Autoload(ref string name, ref string texture) {
      texture = "OriMod/Dusts/SFDust1";
      return base.Autoload(ref name, ref texture);
    }

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
