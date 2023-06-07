using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities; 

/// <summary>
/// Medallion that grants <see cref="Glide"/>.
/// </summary>
public class GlideLevel1 : AbilityMedallionBase {
  public override byte Id => AbilityId.Glide;

  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddIngredient(ItemID.Feather, 10)
      .AddIngredient(ItemID.SoulofNight, 20)
      .Register();
  }
}
