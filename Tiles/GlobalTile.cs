using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Tiles {
  /// <summary>
  /// Used for draw effects, specifically brightening solid areas when the player uses <see cref="Burrow"/>.
  /// </summary>
  public sealed class OriTile : GlobalTile {
    private int InnerRange => 4;
    private int OuterRange => 13;

    private void BurrowEffects(int i, int j, ref Color drawColor, OriPlayer oPlayer) {
      Color orig = drawColor;
      Vector2 playerPos = Main.LocalPlayer.Center / 16;
      float dist = Vector2.Distance(playerPos, new Vector2(i, j)) - InnerRange;
      dist = Utils.Clamp((OuterRange - dist) / OuterRange, 0, 1);
      if (Burrow.CanBurrowAny || oPlayer.abilities.burrow.CanBurrow(Main.tile[i, j])) {
        drawColor = Color.Lerp(orig, Color.White, 0.8f * dist);
      }
      else {
        drawColor = Color.Lerp(orig, Color.White, 0.4f * dist);
      }
      drawColor.A = orig.A;
    }

    public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
      var oPlayer = OriPlayer.Local;
      if (oPlayer.abilities.burrow) {
        BurrowEffects(i, j, ref drawColor, oPlayer);
        if (oPlayer.debugMode) {
          var pos = new Point(i, j);
          if (Burrow.InnerHitbox.Points.Contains(pos)) {
            drawColor = Color.Red;
          }
          else if (Burrow.EnterHitbox.Points.Contains(pos)) {
            drawColor = Color.LimeGreen;
          }
          else if (Burrow.OuterHitbox.Points.Contains(pos)) {
            drawColor = Color.Turquoise;
          }
        }
      }
    }
  }
}
