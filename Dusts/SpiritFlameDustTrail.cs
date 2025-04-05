using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts;

/// <summary>
/// Dust used as a trail for <see cref="Projectiles.Minions.SpiritFlame"/>.
/// </summary>
[UsedImplicitly]
public sealed class SpiritFlameDustTrail : ModDust {
  private int AlphaRate => 1;

  public override string Texture => "OriMod/Dusts/SFDust1";

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
      return false;
    }

    dust.frame.Y = dust.alpha switch {
      > 11 => 40,
      > 8 => 30,
      > 5 => 20,
      > 2 => 10,
      _ => dust.frame.Y
    };
    return false;
  }
}
