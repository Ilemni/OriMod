using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="global::OriMod.Abilities.AirJump"/> Level 3: Quadruple Jump.
  /// </summary>
  public class AirJumpLevel3 : AbilityMedallionBase {
    public override byte ID => AbilityID.AirJump;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HallowedBar, 16);
      recipe.AddIngredient(ItemID.BundleofBalloons);
      recipe.AddRecipe();
    }
  }
}
