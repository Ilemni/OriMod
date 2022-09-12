using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 1.
  /// </summary>
  public class BashLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Bash;

    public override void AddRecipes() {
      GetAbilityRecipe()
        .AddRecipeGroup("OriMod:HardmodeBars1", 16)
        .AddIngredient(ItemID.SoulofLight, 30)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 2: Bashing Projectiles.
  /// </summary>
  public class BashLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Bash;
    public override byte Level => 2;

    public override void AddRecipes() {
      GetAbilityRecipe<BashLevel1>()
        .AddIngredient(ItemID.SpectreBar, 10)
        .AddIngredient(ItemID.SoulofLight, 25)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 3: Ultra Bash.
  /// </summary>
  public class BashLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.Bash;
    public override byte Level => 3;

    public override void AddRecipes() {
      GetAbilityRecipe<BashLevel2>()
        .AddRecipeGroup("OriMod:LunarFragments", 20)
        .AddIngredient(ItemID.SoulofLight, 40)
        .Register();
    }
  }
}
