using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 1.
  /// </summary>
  public class ChargeJumpLevel1 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddIngredient(ItemID.HallowedBar, 18);
      recipe.AddIngredient(ItemID.FrogLeg);
      recipe.AddRecipeGroup("OriMod:JumpBalloons");
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 2: Wall Charge Jump.
  /// </summary>
  public class ChargeJumpLevel2 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;
    public override byte Level => 2;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<ChargeJumpLevel1>();
      recipe.AddIngredient(ItemID.ChlorophyteBar, 22);
      recipe.AddIngredient(ItemID.SoulofFlight, 15);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 3: Launch.
  /// </summary>
  public class ChargeJumpLevel3 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;
    public override byte Level => 3;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<ChargeJumpLevel2>();
      recipe.AddRecipeGroup("OriMod:LunarFragments", 30);
      recipe.AddIngredient(ItemID.SoulofFlight, 25);
      recipe.AddIngredient(ItemID.SoulofLight, 30);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="ChargeJump"/> Level 4: Multi-Launch.
  /// </summary>
  public class ChargeJumpLevel4 : AbilityMedallionBase {
    public override byte ID => AbilityID.ChargeJump;
    public override byte Level => 4;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe<ChargeJumpLevel3>();
      recipe.AddIngredient(ItemID.LunarBar, 24);
      recipe.AddIngredient(ItemID.SoulofFlight, 35);
      recipe.AddIngredient(ItemID.SoulofLight, 50);
      recipe.AddRecipe();
    }
  }
}
