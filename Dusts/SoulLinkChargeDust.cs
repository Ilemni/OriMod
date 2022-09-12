using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  /// <summary>
  /// Originally used for <see cref="Abilities.SoulLink"/>.
  /// </summary>
  [Obsolete]
  // ReSharper disable once ClassNeverInstantiated.Global
  public class SoulLinkChargeDust : ModDust {
    private static int AlphaRate => 16;
    // ReSharper disable once RedundantAssignment
    public override string Texture => @"OriMod/Dusts/AbilityRefreshedDust";

    public override void OnSpawn(Dust dust) {
      dust.alpha = 0;
      dust.rotation = 0;
      dust.noGravity = true;
      dust.velocity = Vector2.Zero;
    }

    public override bool Update(Dust dust) {
      dust.position += ((Player)dust.customData).velocity;
      dust.alpha += AlphaRate;
      if (dust.alpha <= 255) return false;
      dust.alpha = 255;
      dust.active = false;
      return false;
    }
  }
}
