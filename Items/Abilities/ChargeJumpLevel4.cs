using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 4: Multi-Launch.
  /// </summary>
  public class ChargeJumpLevel4 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<ChargeJumpLevel3>();
      recipe.AddIngredient(ItemID.LunarBar, 24);
      recipe.AddIngredient(ItemID.SoulofFlight, 35);
      recipe.AddIngredient(ItemID.SoulofLight, 50);
      recipe.AddRecipe();
    }
  }
}
