using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Glide"/>.
  /// </summary>
  public class GlideLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Glide;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.Feather, 10);
      recipe.AddIngredient(ItemID.SoulofNight, 20);
      recipe.AddRecipe();
    }
  }
}
