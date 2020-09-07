using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 2: Launch.
  /// </summary>
  public class ChargeJumpLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 30);
      recipe.AddIngredient(ItemID.SoulofFlight, 20);
      recipe.AddRecipe();
    }
  }
}
