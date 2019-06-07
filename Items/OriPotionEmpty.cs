using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace OriMod.Items {
  public class OriPotionEmpty : ModItem {
    public override void SetStaticDefaults() { }
    public override void SetDefaults() {
      item.width = 24;
      item.height = 26;
      item.maxStack = 1;
      item.rare = 1;
      item.useAnimation = 10;
      item.useTime = 10;
      item.useStyle = 3;
      item.consumable = false;
    }

    // Note that this item does not work in Multiplayer, but serves as a learning tool for other things.
    public override bool UseItem(Player player) {
      player.GetModPlayer<OriPlayer>().PlayNewSound("SavePoints/checkpointCantPlaceSound");
      return true;
    }
  }
}