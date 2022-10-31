using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 1.
  /// </summary>
  public class BurrowLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Burrow;

    public override void AddRecipes() {
      GetAbilityRecipe()
        .AddIngredient(ItemID.HellstoneBar, 26)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 2.
  /// </summary>
  public class BurrowLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Burrow;
    public override byte Level => 2;

    public override void AddRecipes() {
      GetAbilityRecipe<BurrowLevel1>()
        .AddRecipeGroup(OriRecipeGroups.HardmodeBars3, 22)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Burrow"/> Level 3.
  /// </summary>
  public class BurrowLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.Burrow;
    public override byte Level => 3;

    public override void AddRecipes() {
      GetAbilityRecipe<BurrowLevel2>()
        .AddIngredient(ItemID.ChlorophyteBar, 14)
        .Register();
    }
  }
}
