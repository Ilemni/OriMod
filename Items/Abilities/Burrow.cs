using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="Burrow"/> Level 1.
/// </summary>
public sealed class BurrowLevel1() : AbilityMedallionBase<Burrow>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddIngredient(ItemID.HellstoneBar, 26)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="Burrow"/> Level 2.
/// </summary>
public sealed class BurrowLevel2() : AbilityMedallionBase<Burrow>(2) {
  public override void AddRecipes() {
    GetAbilityRecipe<BurrowLevel1>()
      .AddRecipeGroup(OriRecipeGroups.HardmodeBars3, 22)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="Burrow"/> Level 3.
/// </summary>
public sealed class BurrowLevel3() : AbilityMedallionBase<Burrow>(3) {
  public override void AddRecipes() {
    GetAbilityRecipe<BurrowLevel2>()
      .AddIngredient(ItemID.ChlorophyteBar, 14)
      .Register();
  }
}
