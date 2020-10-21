using OriMod.Buffs;
using OriMod.Projectiles.Minions;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  public class SpiritOrb1 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff1>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein1>();
    protected override int SeinType => 1;

    public override void AddRecipes() {
      var recipe = GetRecipe();
      recipe.AddIngredient(ItemID.SilverBar, 8);
      recipe.AddRecipe();

      recipe = new ModRecipe(mod);
      recipe.AddIngredient(ItemID.TungstenBar, 8);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb2 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff2>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein2>();
    protected override int SeinType => 2;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb1>();
      recipe.AddIngredient(ItemID.DemoniteBar, 12);
      recipe.AddRecipe();

      recipe = GetRecipe<SpiritOrb1>();
      recipe.AddIngredient(ItemID.CrimtaneBar, 12);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb3 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff3>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein3>();
    protected override int SeinType => 3;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb2>();
      recipe.AddIngredient(ItemID.HellstoneBar, 15);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb4 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff4>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein4>();
    protected override int SeinType => 4;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb3>();
      recipe.AddIngredient(ItemID.MythrilBar, 12);
      recipe.AddIngredient(ItemID.SoulofLight, 5);
      recipe.AddRecipe();

      recipe = GetRecipe<SpiritOrb3>();
      recipe.AddIngredient(ItemID.OrichalcumBar, 12);
      recipe.AddIngredient(ItemID.SoulofLight, 5);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb5 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff5>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein5>();
    protected override int SeinType => 5;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb4>();
      recipe.AddIngredient(ItemID.HallowedBar, 15);
      recipe.AddIngredient(ItemID.SoulofLight, 10);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb6 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff6>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein6>();
    protected override int SeinType => 6;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb6>();
      recipe.AddIngredient(ItemID.SpectreBar, 12);
      recipe.AddIngredient(ItemID.SoulofLight, 15);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb7 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff7>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein7>();
    protected override int SeinType => 7;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb6>();
      recipe.AddIngredient(ItemID.FragmentSolar, 5);
      recipe.AddIngredient(ItemID.FragmentVortex, 5);
      recipe.AddIngredient(ItemID.FragmentNebula, 5);
      recipe.AddIngredient(ItemID.FragmentStardust, 5);
      recipe.AddIngredient(ItemID.SoulofLight, 20);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb8 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff8>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein8>();
    protected override int SeinType => 8;

    public override void AddRecipes() {
      var recipe = GetRecipe<SpiritOrb7>();
      recipe.AddIngredient(ItemID.LunarBar, 12);
      recipe.AddIngredient(ItemID.FragmentSolar, 10);
      recipe.AddIngredient(ItemID.FragmentVortex, 10);
      recipe.AddIngredient(ItemID.FragmentNebula, 10);
      recipe.AddIngredient(ItemID.FragmentStardust, 10);
      recipe.AddIngredient(ItemID.SoulofLight, 30);
      recipe.AddRecipe();
    }
  }
}
