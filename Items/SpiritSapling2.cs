using System;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// <see cref="SpiritSapling"/> was intended to be multi-tiered for an Upgrade System.
  /// </summary>
  [Obsolete]
  public class SpiritSapling2 : ModItem {
    public override bool Autoload(ref string name) => false;
    
    public override string Texture => "OriMod/Items/SpiritSapling"; // TODO: Add SpiritSapling2 sprite

    public override void SetDefaults() {
      item.width = 12;
      item.height = 30;
      item.maxStack = 1;
      item.useTurn = true;
      item.autoReuse = true;
      item.useAnimation = 15;
      item.useTime = 10;
      item.useStyle = ItemUseStyleID.SwingThrow;
      item.consumable = true;
      item.value = 150000;
      item.createTile = ModContent.TileType<Tiles.SpiritSapling>(); // TODO: Tiles.SpiritSapling2
    }

    public override void AddRecipes() {
      ModRecipe recipe = new ModRecipe(mod);
      recipe.AddIngredient(ModContent.TileType<Tiles.SpiritSapling>());
      recipe.AddIngredient(ItemID.SoulofLight, 30);
    }
  }
}
