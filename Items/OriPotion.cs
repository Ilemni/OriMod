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
      Item.width = 24;
      Item.height = 26;
      Item.maxStack = 1;
      Item.rare = ItemRarityID.Blue;
      Item.useAnimation = 17;
      Item.useTime = 30;
      Item.useStyle = ItemUseStyleID.EatFood;
      Item.consumable = true;
    }

    public override bool? UseItem(Player player) {
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
      Item.NewItem(player.GetSource_FromThis(), player.getRect(), ModContent.ItemType<OriPotionEmpty>(), noGrabDelay: true);
      return true;
    }

    public override void AddRecipes() {
      CreateRecipe()
        .AddIngredient(ItemID.Bottle)
        .AddIngredient(ItemID.Moonglow)
        .AddIngredient(ItemID.Shiverthorn)
        .AddIngredient(ItemID.Fireblossom)
        .AddTile(ModContent.TileType<Tiles.SpiritSapling>())
        .Register();
      CreateRecipe()
        .AddIngredient(ModContent.ItemType<OriPotionEmpty>())
        .AddTile(ModContent.TileType<Tiles.SpiritSapling>())
        .Register();
    }
  }
}
