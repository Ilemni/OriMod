using OriMod.Abilities;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="WallJump"/>.
/// </summary>
public sealed class WallJumpLevel1() : AbilityMedallionBase<WallJump>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddRecipeGroup(OriRecipeGroups.GoldBars, 15)
      .AddRecipeGroup(OriRecipeGroups.WallJumpGear)
      .Register();
  }
}
