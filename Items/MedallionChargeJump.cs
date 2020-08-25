using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 1.
  /// </summary>
  public class MedallionChargeJump : AbilityMedallion {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HallowedBar, 18);
      recipe.AddIngredient(ItemID.FrogLeg);
      recipe.AddRecipeGroup("OriMod:JumpBalloon");
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 2: Launch.
  /// </summary>
  public class MedallionChargeJumpLevel2 : AbilityMedallion {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 16);
      recipe.AddIngredient(ItemID.SoulofFlight, 20);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 3: Multi-Launch.
  /// </summary>
  public class MedallionChargeJumpLevel3 : AbilityMedallion {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.LunarBar, 30);
      recipe.AddIngredient(ItemID.SoulofFlight, 40);
      recipe.AddIngredient(ItemID.SoulofLight, 25);
      recipe.AddRecipe();
    }
  }
}
