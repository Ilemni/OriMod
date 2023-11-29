using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using OriMod.Dusts;
using OriMod.Utilities;

namespace OriMod.Tiles;

/// <summary>
/// Creates an updraft, which can be used with glide to gain height
/// </summary>
public class HotAshTile : ModTile {
  public override void SetStaticDefaults() {
    Main.tileSolid[Type] = true;
    Main.tileLighted[Type] = true;
    DustType = DustID.Ash;
    TileID.Sets.TouchDamageHot[Type] = true;
    AddMapEntry(new Color(0.733f, 0.188f, 0.2f));
  }

  public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b){
    r = 0.733f; g = 0.188f; b = 0.2f;
  }

  public override void NearbyEffects(int i, int j, bool closer) {
    if (Main.rand.Next(0,30) != 0) return;

    Vector2 position = new Point(i,j-1).ToWorldCoordinates(Main.rand.NextVector2Square(0,16));
    Tile tile = Main.tile[i,j-1];
    if (OriUtils.IsSolid(tile,true)) return;

    int y = Main.rand.Next(0,45);
    for (int z = y; z >= 0; z--) {
      tile = Main.tile[position.ToTileCoordinates() + new Point(0,-z)];
      if (!OriUtils.IsSolid(tile,true)) continue;
      return;
    }

    Dust wind = Dust.NewDustPerfect(position,ModContent.DustType<WindDust>(),new Vector2(0,-1.6f),0,Color.White);
    wind.position.Y -= y*16;
  }
}
