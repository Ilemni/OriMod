using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace OriMod.Items {
  [AutoloadEquip(EquipType.Body)]
  public class DebugArmor : ModItem {
    public override void SetStaticDefaults() {
      base.SetStaticDefaults();
      DisplayName.SetDefault("Debug Armor");
      Tooltip.SetDefault("Used to test Sein.");
    }

    public override void SetDefaults() {
      item.width = 18;
      item.height = 18;
      item.rare = 10;
      item.value = 90000000;
    }

    public override void UpdateEquip(Player player) {
      player.noKnockback = true;
      player.maxMinions++;
      player.immune = true;
    }
    public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
      player.statLife = player.statLifeMax;
    }

    // public override void AddRecipes() {
    //  ModRecipe recipe = new ModRecipe(mod);
    //  recipe.AddIngredient(ItemID.DirtBlock, 1);
    //  recipe.SetResult(this);
    //  recipe.AddRecipe();
    // }
  }
}