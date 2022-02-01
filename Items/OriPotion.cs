using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// Consumable item that toggles <see cref="OriPlayer.IsOri"/>.
  /// </summary>
  public class OriPotion : ModItem {
    public override void SetDefaults() {
      item.width = 24;
      item.height = 26;
      item.maxStack = 1;
      item.rare = ItemRarityID.Blue;
      item.useAnimation = 17;
      item.useTime = 30;
      item.useStyle = ItemUseStyleID.EatingUsing;
      item.consumable = true;
    }

    public override bool UseItem(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.IsOri ^= true;

      Vector2 pos = player.position;
      pos.Y += 4;
      pos.X -= 2;
      for (int m = 0; m < 100; m++) {
        Dust dust = Dust.NewDustDirect(pos, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255));
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      }
      oPlayer.PlaySound("SavePoints/checkpointSpawnSound");
      Item.NewItem(player.getRect(), ModContent.ItemType<OriPotionEmpty>(), noGrabDelay: true);
      return true;
    }

    public override void AddRecipes() {
      ModRecipe recipe = new ModRecipe(mod);
      recipe.AddIngredient(ItemID.Bottle);
      recipe.AddIngredient(ItemID.Moonglow);
      recipe.AddIngredient(ItemID.Shiverthorn);
      recipe.AddIngredient(ItemID.Fireblossom);
      recipe.AddTile(null, "SpiritSapling");
      recipe.SetResult(this);
      recipe.AddRecipe();

      ModRecipe recipe2 = new ModRecipe(mod);
      recipe2.AddIngredient(null, "OriPotionEmpty");
      recipe2.AddTile(null, "SpiritSapling");
      recipe2.SetResult(this);
      recipe2.AddRecipe();
    }
  }
}
