using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.Tiles;

/// <summary>
/// Used for draw effects, specifically brightening solid areas when the player uses <see cref="Burrow"/>.
/// </summary>
[UsedImplicitly]
public sealed class OriTile : GlobalTile {
  private static int InnerRange => 4;
  private static int OuterRange => 13;

  private static void BurrowEffects(int i, int j, ref Color drawColor, OriPlayer oPlayer, Burrow burrow) {
    Color orig = drawColor;
    Vector2 playerPos = oPlayer.Player.Center / 16;
    float dist = Vector2.Distance(playerPos, new Vector2(i, j)) - InnerRange;
    dist = Utils.Clamp((OuterRange - dist) / OuterRange, 0, 1);
    drawColor = Color.Lerp(orig, Color.White,
      (burrow.CanBurrow(Main.tile[i, j]) ? 0.8f : 0.4f) * dist);
    drawColor.A = orig.A;
  }

  public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawInfo) {
    OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
    if (oPlayer.ActiveState is not Burrow burrow) {
      return;
    }

    BurrowEffects(i, j, ref drawInfo.finalColor, oPlayer, burrow);

    if (oPlayer.DebugMode) {
      DebugEffects(i, j, ref drawInfo.finalColor);
    }
  }

  private static void DebugEffects(int i, int j, ref Color drawColor) {
    Point pos = new(i, j);
    if (Burrow.InnerHitbox.Contains(pos)) {
      drawColor = Color.Red;
    }
    else if (Burrow.EnterHitbox.Contains(pos)) {
      drawColor = Color.LimeGreen;
    }
  }
}
