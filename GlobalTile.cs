using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public class OriTile : GlobalTile {
    public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Microsoft.Xna.Framework.Color drawColor, ref int nextSpecialDrawIndex) {
      if (!Main.LocalPlayer.GetModPlayer<OriPlayer>().burrow.InUse) {
        return;
      }
      if (Abilities.Burrow.Burrowable.Contains(type)) {
        drawColor = Color.White * 0.4f;
      }
      else {
        drawColor = Color.White * 0.2f;
      }
    }
  }
}