using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.ID;
using Terraria.Localization;
using Terraria.GameContent.Drawing;
using OriMod.Dusts;

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
  }
  public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b){
    r = 0.929f; g = 0.486f; b = 0.180f;
  }
  public override void NearbyEffects(int i, int j, bool closer) {
    if (Main.rand.Next(0,30) != 0) return;
    Vector2 position = new Point(i,j-1).ToWorldCoordinates(Main.rand.NextVector2Square(0,16));
    Tile tile = Main.tile[i,j-1];
    if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType]) return;
    Dust wind = Dust.NewDustPerfect(position,ModContent.DustType<WindDust>(),new Vector2(0,-1.6f),0,Color.White,1);
    wind.rotation = MathHelper.Pi;
    wind.customData = 150;
    int y = Main.rand.Next(0,45);
    for (int z = y; z >= 0; z--) {
      tile = Main.tile[wind.position.ToTileCoordinates() + new Point(0,-z)];
      if (!tile.HasTile || tile.IsActuated || !Main.tileSolid[tile.TileType]) continue;
      return;
    }
    wind.position.Y -= y*16;
  }
}
