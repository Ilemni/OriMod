using System.Linq;
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
public sealed class OriTile : GlobalTile {
  private static int InnerRange => 4;
  private static int OuterRange => 13;

  private static void BurrowEffects(int i, int j, ref Color drawColor, OriPlayer oPlayer) {
    Color orig = drawColor;
    Vector2 playerPos = Main.LocalPlayer.Center / 16;
    float dist = Vector2.Distance(playerPos, new Vector2(i, j)) - InnerRange;
    dist = Utils.Clamp((OuterRange - dist) / OuterRange, 0, 1);
    drawColor = Color.Lerp(orig, Color.White,
      (oPlayer.abilities.burrow.CanBurrow(Main.tile[i, j]) ? 0.8f : 0.4f) * dist);
    drawColor.A = orig.A;
  }

  public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawInfo) {
    OriPlayer oPlayer = OriPlayer.Local;
    if (!oPlayer.abilities.burrow) return;
    BurrowEffects(i, j, ref drawInfo.finalColor, oPlayer);
    if (!oPlayer.debugMode) return;
    Point pos = new(i, j);
    if (Burrow.InnerHitbox.Points.Contains(pos)) {
      drawInfo.finalColor = Color.Red;
    }
    else if (Burrow.EnterHitbox.Points.Contains(pos)) {
      drawInfo.finalColor = Color.LimeGreen;
    }
  }
}
