using OriMod.Abilities;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="WallJump"/>.
  /// </summary>
  public class WallJumpLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.WallJump;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:GoldBars", 15);
      recipe.AddRecipeGroup("OriMod:WallJumpGear");
      recipe.AddRecipe();
    }
  }
}
