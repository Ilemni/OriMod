using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities;

/// <summary>
/// Medallion that grants <see cref="Climb"/>.
/// </summary>
public sealed class ClimbLevel1() : AbilityMedallionBase<Climb>(1) {
  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddIngredient(ItemID.HellstoneBar, 14)
      .AddIngredient(ItemID.TigerClimbingGear)
      .Register();
  }
}
