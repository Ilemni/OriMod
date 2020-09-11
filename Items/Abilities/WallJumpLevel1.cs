using OriMod.Abilities;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="WallJump"/>.
  /// </summary>
  public class WallJumpLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.WallJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:GoldBars", 15);
      recipe.AddRecipeGroup("OriMod:WallJumpGear");
      recipe.AddRecipe();
    }
  }
}
