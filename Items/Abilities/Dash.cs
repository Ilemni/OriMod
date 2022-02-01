using OriMod.Abilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 1.
  /// </summary>
  public class DashLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Dash;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars1", 14);
      recipe.AddIngredient(ItemID.SoulofLight, 10);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 2: No Cooldown.
  /// </summary>
  public class DashLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Dash;
    public override byte Level => 2;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<DashLevel1>();
      recipe.AddIngredient(ItemID.HallowedBar, 20);
      recipe.AddIngredient(ItemID.SoulofLight, 20);
      recipe.AddRecipe();
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
      ModRecipe recipe = GetAbilityRecipe<DashLevel2>();
      recipe.AddIngredient(ItemID.ShroomiteBar, 10);
      recipe.AddIngredient(ItemID.SoulofLight, 35);
      recipe.AddIngredient(ItemID.Tabi);
      recipe.AddRecipe();
    }
  }
}
