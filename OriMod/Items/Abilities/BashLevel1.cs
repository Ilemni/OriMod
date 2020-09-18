using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 1.
  /// </summary>
  public class BashLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Bash;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars1", 16);
      recipe.AddIngredient(ItemID.SoulofLight, 30); // Bleh, Souls the best I can come up with???
      recipe.AddRecipe();
    }
  }
}
