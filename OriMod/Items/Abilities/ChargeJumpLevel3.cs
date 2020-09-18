using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 3: Launch.
  /// </summary>
  public class ChargeJumpLevel3 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<ChargeJumpLevel2>();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 30);
      recipe.AddIngredient(ItemID.SoulofFlight, 25);
      recipe.AddIngredient(ItemID.SoulofLight, 30);
      recipe.AddRecipe();
    }
  }
}
