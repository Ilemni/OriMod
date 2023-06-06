using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 1.
  /// </summary>
  public class DashLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Dash;

    public override void AddRecipes() {
      GetAbilityRecipe()
        .AddRecipeGroup(OriRecipeGroups.HardmodeBars1, 14)
        .AddIngredient(ItemID.SoulofLight, 10)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 2: No Cooldown.
  /// </summary>
  public class DashLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Dash;
    public override byte Level => 2;

    public override void AddRecipes() {
      GetAbilityRecipe<DashLevel1>()
        .AddIngredient(ItemID.HallowedBar, 20)
        .AddIngredient(ItemID.SoulofLight, 20)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 3: <see cref="ChargeDash"/>.
  /// </summary>
  /// <remarks>
  /// Although <see cref="ChargeDash"/> is a different ability, its unlock condition is that <see cref="Dash"/> is at least Level 3.
  /// </remarks>
  public class DashLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.Dash;
    public override byte Level => 3;

    public override void AddRecipes() {
      GetAbilityRecipe<DashLevel2>()
        .AddIngredient(ItemID.ShroomiteBar, 10)
        .AddIngredient(ItemID.SoulofLight, 35)
        .AddIngredient(ItemID.Tabi)
        .Register();
    }
  }
}
