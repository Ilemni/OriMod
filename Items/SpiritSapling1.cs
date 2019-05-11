using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items
{
	public class SpiritSapling1 : ModItem
	{
		public override void SetStaticDefaults() { }

		public override void SetDefaults()
		{
			item.width = 12;
			item.height = 30;
			item.maxStack = 1;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.value = 150;
			item.createTile = mod.TileType("SpiritSapling1");
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Wood, 10);
			recipe.AddRecipeGroup("OriMod:EnchantedItems", 1);
			recipe.SetResult(this);
			recipe.AddRecipe();

			ModRecipe recipe2 = new ModRecipe(mod);
			recipe2.AddIngredient(ItemID.Wood, 10);
			recipe2.AddRecipeGroup("OriMod:MovementAccessories", 3);
			recipe2.SetResult(this);
			recipe2.AddRecipe();

			// Debug
			ModRecipe debugRecipe = new ModRecipe(mod);
			debugRecipe.AddIngredient(ItemID.DirtBlock, 1);
			debugRecipe.SetResult(this);
			debugRecipe.AddRecipe();
		}
	}
}