using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  public class OriPotion : ModItem {
    public override void SetDefaults() {
      item.width = 24;
      item.height = 26;
      item.maxStack = 1;
      item.rare = 1;
      item.useAnimation = 17;
      item.useTime = 30;
      item.useStyle = 2;
      item.consumable = true;
    }

    public override bool UseItem(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.OriSet = !oPlayer.OriSet;

      for (int m = 0; m < 100; m++) { //does particles
        Vector2 pos = player.position;
        pos.Y += 4;
        pos.X -= 2;
        Dust dust = Main.dust[Terraria.Dust.NewDust(pos, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      }
      player.GetModPlayer<OriPlayer>().PlayNewSound("SavePoints/checkpointSpawnSound");
      Item.NewItem(player.getRect(), mod.ItemType("OriPotionEmpty"), noGrabDelay: true);
      return true;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(ItemID.Bottle);
      recipe.AddIngredient(ItemID.Moonglow);
      recipe.AddIngredient(ItemID.Shiverthorn);
      recipe.AddIngredient(ItemID.Fireblossom);
      recipe.AddTile(null, "SpiritSapling");
      recipe.SetResult(this, 1);
      recipe.AddRecipe();

      var recipe2 = new ModRecipe(mod);
      recipe2.AddIngredient(null, "OriPotionEmpty", 1);
      recipe2.AddTile(null, "SpiritSapling");
      recipe2.SetResult(this, 1);
      recipe2.AddRecipe();
    }
  }
}
