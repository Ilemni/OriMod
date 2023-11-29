using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items;

/// <summary>
/// Used to place <see cref="Tiles.HotAshTile"/>
/// </summary>
public class HotAshItem : ModItem {
  public override void SetDefaults() {
    Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.HotAshTile>());
    ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
  }

  public override void AddRecipes() {
    CreateRecipe()
      .AddIngredient(ItemID.AshBlock)
      .AddIngredient(ItemID.LivingFireBlock)
      .AddTile(TileID.Hellforge)
      .Register();
    CreateRecipe()
      .AddIngredient(ItemID.Wood, 30)
      .AddIngredient(ItemID.LivingFireBlock)
      .AddTile(TileID.Hellforge)
      .Register();
  }
}