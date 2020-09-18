using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 3: Quintuple Jump.
  /// </summary>
  public class AirJumpLevel4 : AbilityMedallionBase {
    public override byte ID => AbilityID.AirJump;
    public override byte Level => 4;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<AirJumpLevel3>();
      recipe.AddIngredient(ItemID.ShroomiteBar, 18);
      recipe.AddIngredient(ItemID.BundleofBalloons);
      recipe.AddRecipe();
    }
  }
}
