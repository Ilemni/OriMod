using OriMod.Abilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Glide"/>.
  /// </summary>
  public class GlideLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Glide;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.Feather, 10);
      recipe.AddIngredient(ItemID.SoulofNight, 20);
      recipe.AddRecipe();
    }
  }
}
