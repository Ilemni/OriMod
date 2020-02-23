using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Tiles {
  public class OriTile : GlobalTile {
    private int InnerRange => 4;
    private int OuterRange => 13;
    private OriPlayer oPlayer => Main.LocalPlayer.GetModPlayer<OriPlayer>();

    private void BurrowEffects(int i, int j, ref Color drawColor, OriPlayer oPlayer) {
      Color orig = drawColor;
      Vector2 playerPos = Main.LocalPlayer.Center / 16;
      float dist = Vector2.Distance(playerPos, new Vector2(i, j)) - InnerRange;
      dist = Utils.Clamp((OuterRange - dist) / OuterRange, 0, 1);
      if (Abilities.Burrow.CanBurrowAny || oPlayer.Abilities.burrow.CanBurrow(Main.tile[i, j])) {
        drawColor = Color.Lerp(orig, Color.White, 0.8f * dist);
      }
      else {
        drawColor = Color.Lerp(orig, Color.White, 0.4f * dist);
      }
      drawColor.A = orig.A;
    }

    public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
      var oPlayer = this.oPlayer;
      var burrow = oPlayer.Abilities.burrow;
      if (burrow.InUse) {
        BurrowEffects(i, j, ref drawColor, oPlayer);
      }
      if (oPlayer.debugMode) {
        var pos = new Point(i, j);
        if (oPlayer.Abilities.burrow.InnerHitbox.Points.Contains(pos)) {
          drawColor = Color.Red;
        }
        else if (burrow.EnterHitbox.Points.Contains(pos)) {
          drawColor = Color.LimeGreen;
        }
        else if (burrow.OuterHitbox.Points.Contains(pos)) {
          drawColor = Color.Turquoise;
        }
      }
    }
  }
}
