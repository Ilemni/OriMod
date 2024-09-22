using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="Bash"/> Level 1.
/// </summary>
public sealed class BashLevel1() : AbilityMedallionBase<Bash>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddRecipeGroup(OriRecipeGroups.HardmodeBars1, 16)
      .AddIngredient(ItemID.SoulofLight, 30)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="Bash"/> Level 2: Bashing Projectiles.
/// </summary>
public sealed class BashLevel2() : AbilityMedallionBase<Bash>(2) {
  public override void AddRecipes() {
    GetAbilityRecipe<BashLevel1>()
      .AddIngredient(ItemID.SpectreBar, 10)
      .AddIngredient(ItemID.SoulofLight, 25)
      .Register();
  }
}

/// <summary>
/// Medallion that grants <see cref="Bash"/> Level 3: Ultra Bash.
/// </summary>
public sealed class BashLevel3() : AbilityMedallionBase<Bash>(3) {
  public override void AddRecipes() {
    GetAbilityRecipe<BashLevel2>()
      .AddRecipeGroup(OriRecipeGroups.LunarFragments, 20)
      .AddIngredient(ItemID.SoulofLight, 40)
      .Register();
  }
}
