using Terraria.ID;
using Terraria.ModLoader;
namespace OriMod.Items {
	public class SpiritOrb1 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(1);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SilverBar, 8);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
			
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.TungstenBar, 8);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb2 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(2);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb1"));
			recipe.AddIngredient(ItemID.DemoniteBar, 12);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
			
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb1"));
			recipe.AddIngredient(ItemID.CrimtaneBar, 12);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb3 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(3);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb2"));
			recipe.AddIngredient(ItemID.HellstoneBar, 15);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb4 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(4);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb3"));
			recipe.AddIngredient(ItemID.MythrilBar, 12);
			recipe.AddIngredient(ItemID.SoulofLight, 5);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
			
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb3"));
			recipe.AddIngredient(ItemID.OrichalcumBar, 12);
			recipe.AddIngredient(ItemID.SoulofLight, 5);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb5 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(5);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb4"));
			recipe.AddIngredient(ItemID.HallowedBar, 15);
			recipe.AddIngredient(ItemID.SoulofLight, 10);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb6 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(6);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb5"));
			recipe.AddIngredient(ItemID.SpectreBar, 12);
			recipe.AddIngredient(ItemID.SoulofLight, 15);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb7 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(7);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb6"));
			recipe.AddIngredient(ItemID.FragmentSolar, 5);
			recipe.AddIngredient(ItemID.FragmentVortex, 5);
			recipe.AddIngredient(ItemID.FragmentNebula, 5);
			recipe.AddIngredient(ItemID.FragmentStardust, 5);
			recipe.AddIngredient(ItemID.SoulofLight, 20);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
	public class SpiritOrb8 : SpiritOrbBase {
    public override void SetDefaults() {
			base.SetDefaults();
			Init(8);
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.GetItem("SpiritOrb7"));
			recipe.AddIngredient(ItemID.LunarBar, 12);
			recipe.AddIngredient(ItemID.FragmentSolar, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 10);
			recipe.AddIngredient(ItemID.FragmentNebula, 10);
			recipe.AddIngredient(ItemID.FragmentStardust, 10);
			recipe.AddIngredient(ItemID.SoulofLight, 30);
			recipe.AddTile(mod.GetTile("SpiritSapling1"));
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
  }
}