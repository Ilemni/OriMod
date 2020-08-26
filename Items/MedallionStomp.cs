using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 1.
  /// </summary>
  public class MedallionStomp : AbilityMedallion {
    public override byte ID => AbilityID.Stomp;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars3", 18);
      recipe.AddIngredient(ItemID.SoulofMight, 12);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 2: More damage, greatly increased knockback.
  /// </summary>
  public class MedallionStompLevel2 : AbilityMedallion {
    public override byte ID => AbilityID.Stomp;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 24);
      recipe.AddIngredient(ItemID.SoulofMight, 18);
      recipe.AddRecipe();
    }
  }
}
