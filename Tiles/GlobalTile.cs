using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Tiles;

/// <summary>
/// Used for draw effects, specifically brightening solid areas when the player uses <see cref="Burrow"/>.
/// </summary>
[UsedImplicitly]
public sealed class OriTile : GlobalWall {
  private static int InnerRange => 4;
  private static int OuterRange => 13;

  public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
    OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
    if (oPlayer.ActiveState is not Burrow burrow) {
      return;
    }

    float lightStrength = burrow.CanBurrow(Main.tile[i, j]) ? 0.8f : 0.4f;

    float dist = (oPlayer.Player.Center / 16 - new Vector2(i, j)).Length();
    if (dist < InnerRange) {
      r = MathHelper.Clamp(r + lightStrength, r, 1f);
      g = MathHelper.Clamp(g + lightStrength, g, 1f);
      b = MathHelper.Clamp(b + lightStrength, b, 1f);
    }
    else if (dist < OuterRange) {
      float lerp = 1 - (dist - InnerRange) / (OuterRange - InnerRange);
      r = MathHelper.Lerp(r, lightStrength, lerp);
      g = MathHelper.Lerp(g, lightStrength, lerp);
      b = MathHelper.Lerp(b, lightStrength, lerp);
    }

    if (oPlayer.DebugMode) {
      Point pos = new(i, j);
      if (Burrow.InnerHitbox.Contains(pos)) {
        r = 1f;
        g = 0f;
        b = 0f;
      }
      else if (Burrow.EnterHitbox.Contains(pos)) {
        r = 0f;
        g = 1f;
        b = 0f;
      }
    }
  }
}
