using OriMod.Abilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Climb"/>.
  /// </summary>
  public class ClimbLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Climb;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HellstoneBar, 14);
      recipe.AddIngredient(ItemID.TigerClimbingGear);
      recipe.AddRecipe();
    }
  }
}
