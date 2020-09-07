using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 3.
  /// </summary>
  public class BurrowLevel3 : AbilityMedallionBase {
    public override byte ID => AbilityID.Burrow;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<BurrowLevel1>();
      recipe.AddIngredient(ItemID.ChlorophyteBar, 28);
      recipe.AddRecipe();
    }
  }
}
