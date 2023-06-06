using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// Crafting station for <see cref="OriPotion"/> and <see cref="SpiritOrb"/>.
  /// </summary>
  public class SpiritSapling : ModItem {
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
      Item.value = 150;
      Item.createTile = ModContent.TileType<Tiles.SpiritSapling>();
    }

    public override void AddRecipes() {
      CreateRecipe()
        .AddIngredient(ItemID.Wood, 10)
        .AddRecipeGroup(OriRecipeGroups.EnchantedItems)
        .Register();
      CreateRecipe()
        .AddIngredient(ItemID.Wood, 10)
        .AddRecipeGroup(OriRecipeGroups.MovementAccessories, 3)
        .Register();
    }
  }
}
