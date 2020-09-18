using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 1.
  /// </summary>
  public class ChargeJumpLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HallowedBar, 18);
      recipe.AddIngredient(ItemID.FrogLeg);
      recipe.AddRecipeGroup("OriMod:JumpBalloons");
      recipe.AddRecipe();
    }
  }
}
