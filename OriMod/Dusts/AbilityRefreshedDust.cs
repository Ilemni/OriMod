using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {
  /// <summary>
  /// Dust used to display that a player's <see cref="Abilities.Ability"/> has just refreshed.
  /// </summary>
  public class AbilityRefreshedDust : ModDust {
    private int alphaRate => 12;
    private int speed => 3;

    public override void OnSpawn(Dust dust) {
      dust.alpha = 0;
      dust.noGravity = true;
      dust.velocity = Vector2.UnitX.RotateRandom(2 * Math.PI) * speed;
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
