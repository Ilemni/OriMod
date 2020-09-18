using OriMod.Abilities;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 1.
  /// </summary>
  public class AirJumpLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.AirJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:DarkBars", 12);
      recipe.AddRecipeGroup("OriMod:JumpBottles");
      recipe.AddRecipe();
    }
  }
}
