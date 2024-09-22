using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="AirJump"/> Level 1.
/// </summary>
public sealed class AirJumpLevel1() : AbilityMedallionBase<AirJump>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddRecipeGroup(OriRecipeGroups.DarkBars, 12)
      .AddRecipeGroup(OriRecipeGroups.JumpBottles)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="AirJump"/> Level 2: Triple Jump.
/// </summary>
public sealed class AirJumpLevel2() : AbilityMedallionBase<AirJump>(2) {
  public override void AddRecipes() {
    GetAbilityRecipe<AirJumpLevel1>()
      .AddRecipeGroup(OriRecipeGroups.HardmodeBars2, 12)
      .AddRecipeGroup(OriRecipeGroups.JumpBalloons)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="AirJump"/> Level 3: Quadruple Jump.
/// </summary>
public sealed class AirJumpLevel3() : AbilityMedallionBase<AirJump>(3) {
  public override void AddRecipes() {
    GetAbilityRecipe<AirJumpLevel2>()
      .AddIngredient(ItemID.HallowedBar, 16)
      .AddIngredient(ItemID.BundleofBalloons)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="AirJump"/> Level 4: Quintuple Jump.
/// </summary>
public sealed class AirJumpLevel4() : AbilityMedallionBase<AirJump>(4) {
  public override void AddRecipes() {
    GetAbilityRecipe<AirJumpLevel3>()
      .AddIngredient(ItemID.ShroomiteBar, 8)
      .AddIngredient(ItemID.BundleofBalloons)
      .Register();
  }
}
