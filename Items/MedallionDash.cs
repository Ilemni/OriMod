using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 1.
  /// </summary>
  public class MedallionDash : AbilityMedallion {
    public override byte ID => AbilityID.Dash;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars2", 14);
      recipe.AddIngredient(ItemID.SoulofLight, 15);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 2: No Cooldown.
  /// </summary>
  public class MedallionDashLevel2 : AbilityMedallion {
    public override byte ID => AbilityID.Dash;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HallowedBar, 6);
      recipe.AddIngredient(ItemID.SoulofLight, 25);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Dash"/> Level 3: <see cref="ChargeDash"/>.
  /// </summary>
  /// <remarks>
  /// Although <see cref="ChargeDash"/> is a different ability, its unlock condition is that <see cref="Dash"/> is at least Level 3.
  /// </remarks>
  public class MedallionDashLevel3 : AbilityMedallion {
    public override byte ID => AbilityID.Dash;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.ChlorophyteBar, 6);
      recipe.AddIngredient(ItemID.SoulofLight, 25);
      recipe.AddIngredient(ItemID.Tabi);
      recipe.AddRecipe();
    }
  }
}
