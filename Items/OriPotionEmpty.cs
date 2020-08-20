using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace OriMod.Items {
  public class OriPotionEmpty : ModItem {
    public override void SetDefaults() {
      item.width = 24;
      item.height = 26;
      item.maxStack = 1;
      item.rare = ItemRarityID.Blue;
      item.useAnimation = 10;
      item.useTime = 10;
      item.useStyle = ItemUseStyleID.Stabbing;
      item.consumable = false;
    }

    public override bool UseItem(Player player) {
      if (player.whoAmI == Main.myPlayer) {
        player.GetModPlayer<OriPlayer>().PlayNewSound("SavePoints/checkpointCantPlaceSound");
      }
      return true;
    }
  }
}
