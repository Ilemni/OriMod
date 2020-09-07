using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="global::OriMod.Abilities.Bash"/> Level 2 (deals damage, general increase of stats).
  /// </summary>
  public class BashLevel2 : AbilityMedallionBase {
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
