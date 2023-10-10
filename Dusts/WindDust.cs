using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts; 

/// <summary>
/// Dust used to by Hot Ash blocks to show updrafts for gliding
/// </summary>
public class WindDust : ModDust {
  public override void OnSpawn(Dust dust) {
    dust.alpha = 200;
    dust.noGravity = true;
    dust.customData = 150;
    dust.frame = new Rectangle(0,0,1,64);
  }

  public override bool Update(Dust dust) {
    dust.position += dust.velocity;
    int timer = (int)(dust.customData) - 1;
    dust.customData = timer;
    Tile tile = Main.tile[dust.position.ToTileCoordinates()];
    dust.alpha = 255 - (int)(255f * ((150-timer)/150f) * (timer/150f) * 4f);
    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType]) dust.active = false;
    if (timer <= 0) dust.active = false;
    return false;
  }
}