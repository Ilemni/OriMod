using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 2: More damage, greatly increased knockback.
  /// </summary>
  public class StompLevel2 : AbilityMedallionBase {
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
