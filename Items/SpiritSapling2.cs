using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// <see cref="SpiritSapling"/> was intended to be multi-tiered for an Upgrade System.
  /// </summary>
  [System.Obsolete]
  public class SpiritSapling2 : ModItem {
    public override bool Autoload(ref string name) => false;
    
    public override string Texture => "OriMod/Items/SpiritSapling"; // TODO: Add SpiritSapling2 sprite

    public override void SetDefaults() {
      item.width = 12;
      item.height = 30;
      item.maxStack = 1;
      item.useTurn = true;
      item.autoReuse = true;
      item.useAnimation = 15;
      item.useTime = 10;
      item.useStyle = ItemUseStyleID.SwingThrow;
      item.consumable = true;
      item.value = 150000;
      item.createTile = mod.TileType("SpiritSapling2");
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.ItemType("SpiritSapling"), 1);
      recipe.AddIngredient(ItemID.SoulofLight, 30);
    }
  }
}
