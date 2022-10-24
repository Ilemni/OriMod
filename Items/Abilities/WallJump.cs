using OriMod.Abilities;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="WallJump"/>.
  /// </summary>
  public class WallJumpLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.WallJump;

    public override void AddRecipes() {
      GetAbilityRecipe()
        .AddRecipeGroup("OriMod:GoldBars", 15)
        .AddRecipeGroup("OriMod:WallJumpGear")
        .Register();
    }
  }
}
