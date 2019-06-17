using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public class OriTile : GlobalTile {
    private void BurrowEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex, OriPlayer oPlayer) {
      if (Abilities.Burrow.Burrowable.Contains(type)) {
        drawColor = Color.White * 0.4f;
      }
      else {
        drawColor = Color.White * 0.1f;
      }
      if (oPlayer.debugMode) {
        Vector2[] posArr = oPlayer.burrow.Hitbox;
        Vector2 pos = new Vector2(i, j);
        if (posArr.Contains(pos)) {
          drawColor = Color.Red;
        }
      }
    }
    public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
      OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
      if (oPlayer.debugMode) {
        Vector2[] posArr = oPlayer.burrow.BurrowBox;
        Vector2 pos = new Vector2(i, j);
        if (posArr.Contains(pos)) {
          drawColor = Color.LimeGreen;
        }
      }
      if (oPlayer.burrow.InUse) {
        BurrowEffects(i, j, type, spriteBatch, ref drawColor, ref nextSpecialDrawIndex, oPlayer);
      }
    }
  }
}