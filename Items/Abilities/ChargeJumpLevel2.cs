using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 2: Wall Charge Jump.
  /// </summary>
  public class ChargeJumpLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<ChargeJumpLevel1>();
      recipe.AddRecipeGroup(ItemID.ChlorophyteBar, 22);
      recipe.AddIngredient(ItemID.SoulofFlight, 15);
      recipe.AddRecipe();
    }
  }
}
