using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="Glide"/>.
/// </summary>
public sealed class GlideLevel1() : AbilityMedallionBase<Glide>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddIngredient(ItemID.Feather, 10)
      .AddIngredient(ItemID.SoulofNight, 20)
      .Register();
  }
}
