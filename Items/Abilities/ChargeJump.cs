using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="ChargeJump"/> Level 1.
/// </summary>
public sealed class ChargeJumpLevel1() : AbilityMedallionBase<ChargeJump>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddIngredient(ItemID.HallowedBar, 18)
      .AddIngredient(ItemID.FrogLeg)
      .AddRecipeGroup(OriRecipeGroups.JumpBalloons)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="ChargeJump"/> Level 2: Wall Charge Jump.
/// </summary>
public sealed class ChargeJumpLevel2() : AbilityMedallionBase<WallChargeJump>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe<ChargeJumpLevel1>()
      .AddIngredient(ItemID.ChlorophyteBar, 12)
      .AddIngredient(ItemID.SoulofFlight, 15)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="Launch"/> Level 1.
/// </summary>
public sealed class ChargeJumpLevel3() : AbilityMedallionBase<Launch>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe<ChargeJumpLevel2>()
      .AddRecipeGroup(OriRecipeGroups.LunarFragments, 30)
      .AddIngredient(ItemID.SoulofFlight, 25)
      .AddIngredient(ItemID.SoulofLight, 30)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="ChargeJump"/> Level 4: Multi-Launch.
/// </summary>
public sealed class ChargeJumpLevel4() : AbilityMedallionBase<ChargeJump>(2) {
  public override void AddRecipes() {
    GetAbilityRecipe<ChargeJumpLevel3>()
      .AddIngredient(ItemID.LunarBar, 24)
      .AddIngredient(ItemID.SoulofFlight, 35)
      .AddIngredient(ItemID.SoulofLight, 50)
      .Register();
  }
}
