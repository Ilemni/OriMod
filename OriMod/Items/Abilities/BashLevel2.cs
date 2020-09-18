using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 2: Bashing Projectiles.
  /// </summary>
  public class BashLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.Bash;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<BashLevel1>();
      recipe.AddIngredient(ItemID.SpectreBar, 22);
      recipe.AddIngredient(ItemID.SoulofLight, 25);
      recipe.AddRecipe();
    }
  }
}
