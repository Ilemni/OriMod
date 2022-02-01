﻿using OriMod.Abilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 1.
  /// </summary>
  public class AirJumpLevel1 : AbilityMedallionBase {
    public override byte Id => AbilityId.AirJump;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:DarkBars", 12);
      recipe.AddRecipeGroup("OriMod:JumpBottles");
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 2: Triple Jump.
  /// </summary>
  public class AirJumpLevel2 : AbilityMedallionBase {
    public override byte Id => AbilityId.AirJump;
    public override byte Level => 2;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<AirJumpLevel1>();
      recipe.AddRecipeGroup("OriMod:HardmodeBars2", 12);
      recipe.AddRecipeGroup("OriMod:JumpBalloons");
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 3: Quadruple Jump.
  /// </summary>
  public class AirJumpLevel3 : AbilityMedallionBase {
    public override byte Id => AbilityId.AirJump;
    public override byte Level => 3;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<AirJumpLevel2>();
      recipe.AddIngredient(ItemID.HallowedBar, 16);
      recipe.AddIngredient(ItemID.BundleofBalloons);
      recipe.AddRecipe();
    }
  }

  /// <summary>
  /// Medallion that grants <see cref="AirJump"/> Level 3: Quintuple Jump.
  /// </summary>
  public class AirJumpLevel4 : AbilityMedallionBase {
    public override byte Id => AbilityId.AirJump;
    public override byte Level => 4;

    public override void AddRecipes() {
      ModRecipe recipe = GetAbilityRecipe<AirJumpLevel3>();
      recipe.AddIngredient(ItemID.ShroomiteBar, 8);
      recipe.AddIngredient(ItemID.BundleofBalloons);
      recipe.AddRecipe();
    }
  }
}
