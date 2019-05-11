using Terraria;
using Terraria.ModLoader;

namespace OriMod.Items.Ori {
  public class OriAccessory : ModItem {
    public override void SetDefaults() {
      item.width = 18;
      item.height = 18;
      item.rare = 1;
      item.accessory = true;
    }

    public override void SetStaticDefaults() {
      Tooltip.SetDefault("It's a naturally formed pendant with a small white feather in the center...");
      DisplayName.SetDefault("Feathered Pendant");
    }
    public override void UpdateAccessory(Player player, bool hideVisual) {
      /*
      player.GetModPlayer<OriPlayer>().OriSet = true;
      player.GetModPlayer<OriPlayer>().OriItemOn = true;
      Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0f, 0.39f, 0.5f);
      */
    }
  }
}