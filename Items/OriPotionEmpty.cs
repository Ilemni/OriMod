using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items; 

/// <summary>
/// Empty bottle, craftable into <see cref="OriPotion"/>.
/// </summary>
public class OriPotionEmpty : ModItem {
  public override void SetDefaults() {
    Item.width = 24;
    Item.height = 26;
    Item.maxStack = 1;
    Item.rare = ItemRarityID.Blue;
    Item.useAnimation = 10;
    Item.useTime = 10;
    Item.useStyle = ItemUseStyleID.Thrust;
    Item.consumable = false;
  }

  public override bool? UseItem(Player player) {
    if (player.whoAmI == Main.myPlayer) {
      player.GetModPlayer<OriPlayer>().PlaySound("SavePoints/checkpointCantPlaceSound");
    }
    return true;
  }
}
