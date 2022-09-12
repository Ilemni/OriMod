using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 1.
  /// </summary>
  public class StompLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.Stomp;

    public override void AddRecipes() {
      GetAbilityRecipe()
        .AddRecipeGroup("OriMod:HardmodeBars3", 18)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 2: More damage, greatly increased knockback.
  /// </summary>
  public class StompLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.Stomp;
    public override byte Level => 2;

    public override void AddRecipes() {
      GetAbilityRecipe<StompLevel1>()
        .AddIngredient(ItemID.ShroomiteBar, 12)
        .AddIngredient(ItemID.SoulofMight, 18)
        .Register();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="Stomp"/> Level 3: Basically Lv2 but more
  /// </summary>
  public class StompLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.Stomp;
    public override byte Level => 3;

    public override void AddRecipes() {
      GetAbilityRecipe<StompLevel2>()
        .AddRecipeGroup("OriMod:LunarFragments", 24)
        .AddIngredient(ItemID.SoulofMight, 22)
        .Register();
    }
  }
}
