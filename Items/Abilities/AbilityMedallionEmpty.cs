using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Base material to craft into various <see cref="AbilityMedallionBase"/>s.
  /// </summary>
  public sealed class AbilityMedallionEmpty : ModItem {
    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddRecipeGroup("OriMod:IronBars", 8);
      recipe.AddTile(ModContent.TileType<Tiles.SpiritSapling>());
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }
}
