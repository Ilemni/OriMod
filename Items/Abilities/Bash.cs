using OriMod.Abilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 1.
  /// </summary>
  public class BashLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Bash;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars1", 16);
      recipe.AddIngredient(ItemID.SoulofLight, 30); // Bleh, Souls the best I can come up with???
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 2: Bashing Projectiles.
  /// </summary>
  public class BashLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Bash;
    public override byte Level => 2;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<BashLevel1>();
      recipe.AddIngredient(ItemID.SpectreBar, 10);
      recipe.AddIngredient(ItemID.SoulofLight, 25);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Bash"/> Level 3: Ultra Bash.
  /// </summary>
  public class BashLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.Bash;
    public override byte Level => 3;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<BashLevel2>();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 20);
      recipe.AddIngredient(ItemID.SoulofLight, 40);
      recipe.AddRecipe();
    }
  }
}
