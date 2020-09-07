using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 2: No Cooldown.
  /// </summary>
  public class DashLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.Dash;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<DashLevel1>();
      recipe.AddIngredient(ItemID.HallowedBar, 6);
      recipe.AddIngredient(ItemID.SoulofLight, 25);
      recipe.AddRecipe();
    }
  }
}
