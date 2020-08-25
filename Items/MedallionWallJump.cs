using OriMod.Abilities;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="WallJump"/>.
  /// </summary>
  public class MedallionWallJump : AbilityMedallion {
    public override byte ID => AbilityID.WallJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:GoldBars", 6);
      recipe.AddRecipeGroup("OriMod:WallJumpGear", 6);
      recipe.AddRecipe();
    }
  }
}
