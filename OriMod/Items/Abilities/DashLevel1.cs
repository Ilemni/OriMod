using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 1.
  /// </summary>
  public class DashLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Dash;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars1", 14);
      recipe.AddIngredient(ItemID.SoulofLight, 10);
      recipe.AddRecipe();
    }
  }
}
