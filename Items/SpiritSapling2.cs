using System;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// <see cref="SpiritSapling"/> was intended to be multi-tiered for an Upgrade System.
  /// </summary>
  [Obsolete, Autoload(false)]
    public class SpiritSapling2 : ModItem {    
    public override string Texture => "OriMod/Items/SpiritSapling"; // TODO: Add SpiritSapling2 sprite

    public override void SetDefaults() {
      Item.width = 12;
      Item.height = 30;
      Item.maxStack = 1;
      Item.useTurn = true;
      Item.autoReuse = true;
      Item.useAnimation = 15;
      Item.useTime = 10;
      Item.useStyle = ItemUseStyleID.Swing;
      Item.consumable = true;
      Item.value = 150000;
      Item.createTile = ModContent.TileType<Tiles.SpiritSapling>(); // TODO: Tiles.SpiritSapling2
    }

    public override void AddRecipes() {
      CreateRecipe()
        .AddIngredient(ModContent.TileType<Tiles.SpiritSapling>())
        .AddIngredient(ItemID.SoulofLight, 30)
        .Register();
    }
  }
}
