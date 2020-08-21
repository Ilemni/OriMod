using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  /// <summary>
  /// Originally used for <see cref="Abilities.SoulLink"/>.
  /// </summary>
  [System.Obsolete]
  public class SoulLinkChargeDust : ModDust {
    protected int alphaRate = 16;
    public override bool Autoload(ref string name, ref string texture) {
      texture = "OriMod/Dusts/AbilityRefreshedDust";
      //return base.Autoload(ref name, ref texture);
      return false;
    }

    public override void OnSpawn(Dust dust) {
      dust.alpha = 0;
      dust.rotation = 0;
      dust.noGravity = true;
      dust.velocity = Vector2.Zero;
    }

    public override bool Update(Dust dust) {
      dust.position += (dust.customData as Player).velocity;
      dust.alpha += alphaRate;
      if (dust.alpha > 255) {
        dust.alpha = 255;
        dust.active = false;
      }
      return false;
    }
  }
}
