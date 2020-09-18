using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 3: Ultra Bash.
  /// </summary>
  public class BashLevel3 : AbilityMedallionBase {
    public override byte ID => AbilityID.Bash;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<BashLevel2>();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 20);
      recipe.AddIngredient(ItemID.SoulofLight, 40);
      recipe.AddRecipe();
    }
  }
}
