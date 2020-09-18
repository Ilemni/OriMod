using OriMod.Abilities;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 2.
  /// </summary>
  public class BurrowLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.Burrow;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<BurrowLevel1>();
      recipe.AddRecipeGroup("OriMod:HardmodeBars3", 22);
      recipe.AddRecipe();
    }
  }
}
