using OriMod.Abilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 1.
  /// </summary>
  public class BurrowLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Burrow;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HellstoneBar, 26);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 2.
  /// </summary>
  public class BurrowLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Burrow;
    public override byte Level => 2;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<BurrowLevel1>();
      recipe.AddRecipeGroup("OriMod:HardmodeBars3", 22);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 3.
  /// </summary>
  public class BurrowLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.Burrow;
    public override byte Level => 3;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<BurrowLevel2>();
      recipe.AddIngredient(ItemID.ChlorophyteBar, 14);
      recipe.AddRecipe();
    }
  }
}
