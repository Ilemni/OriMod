using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 1.
  /// </summary>
  public class MedallionAirJump : AbilityMedallion {
    public override byte ID => AbilityID.AirJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:DarkBars", 12);
      recipe.AddRecipeGroup("OriMod:JumpBottles");
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 2: Triple Jump.
  /// </summary>
  public class MedallionAirJumpLevel2 : AbilityMedallion {
    public override byte ID => AbilityID.AirJump;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars1", 12);
      recipe.AddRecipeGroup("OriMod:JumpBalloons");
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 3: Quadruple Jump.
  /// </summary>
  public class MedallionAirJumpLevel3 : AbilityMedallion {
    public override byte ID => AbilityID.AirJump;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.SpectreBar, 16);
      recipe.AddIngredient(ItemID.BundleofBalloons);
      recipe.AddRecipe();
    }
  }
}
