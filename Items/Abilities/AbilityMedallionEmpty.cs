using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Base material to craft into various <see cref="AbilityMedallionBase"/>s.
  /// </summary>
  public sealed class AbilityMedallionEmpty : ModItem {
    public override void AddRecipes() {
      CreateRecipe()
        .AddRecipeGroup("OriMod:IronBars", 8)
        .AddTile(ModContent.TileType<Tiles.SpiritSapling>())
        .Register();
    }
  }
}
