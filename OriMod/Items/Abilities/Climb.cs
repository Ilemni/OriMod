using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Climb"/>.
  /// </summary>
  public class ClimbLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Climb;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HellstoneBar, 14);
      recipe.AddIngredient(ItemID.TigerClimbingGear);
      recipe.AddRecipe();
    }
  }
}
