﻿using OriMod.Abilities;
using Terraria.ID;

namespace OriMod.Items {
  /// <summary>
  /// Medallion that grants <see cref="Climb"/>.
  /// </summary>
  public class MedallionClimb : AbilityMedallion {
    public override byte ID => AbilityID.Climb;

    public override void AddRecipes() {
      var recipe = GetAbilityRecipe();
      recipe.AddRecipeGroup("OriMod:HardmodeBars2", 12);
      recipe.AddIngredient(ItemID.TigerClimbingGear);
      recipe.AddRecipe();
    }
  }
}