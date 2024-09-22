using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts;

/// <summary>
/// Dust used to display that a player's <see cref="AnimLib.Abilities.Ability"/> has just refreshed.
/// </summary>
[UsedImplicitly]
public sealed class AbilityRefreshedDust : ModDust {
  private static int AlphaRate => 12;
  private static int Speed => 3;

  public override void OnSpawn(Dust dust) {
    dust.alpha = 0;
    dust.noGravity = true;
    dust.velocity = Vector2.UnitX.RotateRandom(2 * Math.PI) * Speed;
  }

  public override bool Update(Dust dust) {
    dust.position += dust.velocity;
    dust.alpha += AlphaRate;
    if (dust.alpha <= 255) return false;
    dust.alpha = 255;
    dust.active = false;
    return false;
  }
}
