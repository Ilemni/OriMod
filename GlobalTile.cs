using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public class OriTile : GlobalTile {
    private void BurrowEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex, OriPlayer oPlayer) {
      byte oldAlpha = drawColor.A;
      if (Abilities.Burrow.Burrowable.Contains(type)) {
        drawColor = Color.White * 0.3f;
      }
      else if (Main.tileSolid[type]) {
        drawColor = Color.White * 0.075f;
      }
      drawColor.A = oldAlpha;
      if (oPlayer.debugMode) {
        Point[] posArr = oPlayer.burrow.Hitbox;
        Point pos = new Point(i, j);
        if (posArr.Contains(pos)) {
          drawColor = Color.Red;
        }
      }
    }
    public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
      OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
      if (oPlayer.burrow.InUse) {
        BurrowEffects(i, j, type, spriteBatch, ref drawColor, ref nextSpecialDrawIndex, oPlayer);
      }
      if (oPlayer.debugMode) {
        Point[] posArr = oPlayer.burrow.BurrowBox;
        Point pos = new Point(i, j);
        if (posArr.Contains(pos)) {
          drawColor = Color.LimeGreen;
        }
      }
    }
  }
}