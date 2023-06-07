using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities; 

/// <summary>
/// Medallion that grants <see cref="AirJump"/> Level 1.
/// </summary>
public class AirJumpLevel1 : AbilityMedallionBase {
  public override byte Id => AbilityId.AirJump;

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
public class AirJumpLevel2 : AbilityMedallionBase {
  public override byte Id => AbilityId.AirJump;
  public override byte Level => 2;

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
public class AirJumpLevel3 : AbilityMedallionBase {
  public override byte Id => AbilityId.AirJump;
  public override byte Level => 3;

  public override void AddRecipes() {
    GetAbilityRecipe<AirJumpLevel2>()
      .AddIngredient(ItemID.HallowedBar, 16)
      .AddIngredient(ItemID.BundleofBalloons)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="AirJump"/> Level 3: Quintuple Jump.
/// </summary>
public class AirJumpLevel4 : AbilityMedallionBase {
  public override byte Id => AbilityId.AirJump;
  public override byte Level => 4;

  public override void AddRecipes() {      
    GetAbilityRecipe<AirJumpLevel3>()
      .AddIngredient(ItemID.ShroomiteBar, 8)
      .AddIngredient(ItemID.BundleofBalloons)
      .Register();
  }
}
