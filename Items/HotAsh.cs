using rail;
using Terraria;
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
      .AddIngredient(ItemID.AshBlock, 1)
      .AddIngredient(ItemID.LivingFireBlock, 1)
      .AddTile(TileID.Hellforge)
      .Register();
    CreateRecipe()
      .AddIngredient(ItemID.Wood, 30)
      .AddIngredient(ItemID.Torch, 5)
      .AddTile(TileID.Hellforge)
      .Register();
  }
}