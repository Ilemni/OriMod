using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using OriMod.Utilities;

namespace OriMod.Dusts;

/// <summary>
/// Dust used to by Hot Ash blocks to show updrafts for gliding
/// </summary>
[UsedImplicitly]
public sealed class WindDust : ModDust {
  public override void OnSpawn(Dust dust) {
    dust.alpha = 200;
    dust.noGravity = true;
    dust.customData = 150;
    dust.frame = new Rectangle(0, 0, 1, 64);
  }

  public override bool Update(Dust dust) {
    dust.position += dust.velocity;
    UpdateTimer(dust, out int timer);

    dust.alpha = 255 - (int)(255f * ((150 - timer) / 150f) * (timer / 150f) * 4f);
    if (timer <= 0) {
      dust.active = false;
    }

    return false;
  }

  private static void UpdateTimer(Dust dust, out int timer) {
    Tile tile = Main.tile[dust.position.ToTileCoordinates()];

    timer = (int)dust.customData - 1;
    if (OriUtils.IsSolid(tile, true)) {
      timer = (int)(timer * 0.8f);
    }

    dust.customData = timer;
  }
}
