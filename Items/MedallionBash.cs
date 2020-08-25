using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 1.
  /// </summary>
  public class MedallionBash : AbilityMedallion {
    public override byte ID => AbilityID.Bash;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HallowedBar, 16);
      recipe.AddIngredient(ItemID.SoulofLight, 30); // Bleh, Souls the best I can come up with???
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 2 (deals damage, general increase of stats).
  /// </summary>
  public class MedallionBashLevel2 : AbilityMedallion {
    public override byte ID => AbilityID.Bash;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 18);
      recipe.AddIngredient(ItemID.SoulofLight, 40);
      recipe.AddRecipe();
    }
  }
}
