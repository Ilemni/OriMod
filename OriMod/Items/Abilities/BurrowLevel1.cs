using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 1.
  /// </summary>
  public class BurrowLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Burrow;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HellstoneBar, 26);
      recipe.AddRecipe();
    }
  }
}
