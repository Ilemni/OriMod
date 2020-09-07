using OriMod.Abilities;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 1.
  /// </summary>
  public class BurrowLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Burrow;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:DarkBars", 26);
      recipe.AddRecipe();
    }
  }
}
