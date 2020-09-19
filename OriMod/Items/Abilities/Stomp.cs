using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 1.
  /// </summary>
  public class StompLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.Stomp;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars3", 18);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 2: More damage, greatly increased knockback.
  /// </summary>
  public class StompLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.Stomp;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<StompLevel1>();
      recipe.AddIngredient(ItemID.ShroomiteBar, 12);
      recipe.AddIngredient(ItemID.SoulofMight, 18);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 3: Basically Lv2 but more
  /// </summary>
  public class StompLevel3 : AbilityMedallionBase {
    public override byte ID => AbilityID.Stomp;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<StompLevel2>();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 24);
      recipe.AddIngredient(ItemID.SoulofMight, 22);
      recipe.AddRecipe();
    }
  }
}
