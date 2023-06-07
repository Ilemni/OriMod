using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities; 

/// <summary>
/// Medallion that grants <see cref="Climb"/>.
/// </summary>
public class ClimbLevel1 : AbilityMedallionBase {
  public override byte Id => AbilityId.Climb;

  public override void AddRecipes() {
    GetAbilityRecipe()
      .AddIngredient(ItemID.HellstoneBar, 14)
      .AddIngredient(ItemID.TigerClimbingGear)
      .Register();
  }
}
