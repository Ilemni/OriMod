using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// Crafting station for <see cref="OriPotion"/> and <see cref="SpiritOrb"/>.
  /// </summary>
  public class SpiritSapling : ModItem {
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
      item.value = 150;
      item.createTile = ModContent.TileType<Tiles.SpiritSapling>();
    }

    public override void AddRecipes() {
      ModRecipe recipe = new ModRecipe(mod);
      recipe.AddIngredient(ItemID.Wood, 10);
      recipe.AddRecipeGroup("OriMod:EnchantedItems");
      recipe.SetResult(this);
      recipe.AddRecipe();

      ModRecipe recipe2 = new ModRecipe(mod);
      recipe2.AddIngredient(ItemID.Wood, 10);
      recipe2.AddRecipeGroup("OriMod:MovementAccessories", 3);
      recipe2.SetResult(this);
      recipe2.AddRecipe();
    }
  }
}
