using OriMod.Abilities;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 2: Triple Jump.
  /// </summary>
  public class AirJumpLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.AirJump;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<AirJumpLevel1>();
      recipe.AddRecipeGroup("OriMod:HardmodeBars1", 12);
      recipe.AddRecipeGroup("OriMod:JumpBalloons");
      recipe.AddRecipe();
    }
  }
}
