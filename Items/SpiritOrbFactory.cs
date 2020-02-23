using Microsoft.Xna.Framework;
using OriMod.Buffs;
using OriMod.Projectiles.Minions;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  public class SpiritOrb1 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff1>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein1>();

    internal override SpiritOrb SetOrbDefaults() {
      damage = 12;
      primaryDamageMultiplier = 1;
      targets = 1;
      shotsPerBurst = 2;
      shotsPerTarget = 1;
      shotsToPrimaryTarget = 1;
      maxShotsPerVolley = 1;
      minCooldown = 12f;
      shortCooldown = 24f;
      longCooldown = 40f;
      randDegrees = 40;
      targetMaxDist = 240f;
      targetThroughWallDist = 80f;
      pierce = 1;
      knockback = 0f;
      homingStrengthStart = 0.07f;
      homingIncreaseRate = 0.04f;
      homingIncreaseDelay = 16;
      projectileSpeedStart = 5f;
      projectileSpeedIncreaseRate = 0.5f;
      projectileSpeedIncreaseDelay = 10;
      minionWidth = 10;
      minionHeight = 11;
      flameWidth = 12;
      flameHeight = 12;
      dustScale = 0.8f;
      rarity = 1;
      value = 1000;
      color = default;
      lightStrength = 1;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(ItemID.SilverBar, 8);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();

      recipe = new ModRecipe(mod);
      recipe.AddIngredient(ItemID.TungstenBar, 8);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb2 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff2>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein2>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb1().SetOrbDefaults());
      rarity = 2;
      value = 3000;
      color = new Color(108, 92, 172);
      damage = 17;
      shotsPerBurst = 3;
      projectileSpeedStart = 7f;
      homingIncreaseRate = 0.045f;
      dustScale = 1.3f;
      lightStrength = 1.6f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb1"));
      recipe.AddIngredient(ItemID.DemoniteBar, 12);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();

      recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb1"));
      recipe.AddIngredient(ItemID.CrimtaneBar, 12);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb3 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff3>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein3>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb2().SetOrbDefaults());
      rarity = 3;
      value = 10000;
      color = new Color(240, 0, 0, 194);
      damage = 28;
      targets = 2;
      maxShotsPerVolley = 2;
      shotsPerBurst = 3;
      randDegrees = 100;
      projectileSpeedStart = 10.5f;
      projectileSpeedIncreaseRate = 0.65f;
      projectileSpeedIncreaseDelay = 19;
      targetMaxDist = 370f;
      dustScale = 1.55f;
      lightStrength = 1.275f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb2"));
      recipe.AddIngredient(ItemID.HellstoneBar, 15);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb4 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff4>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein4>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb3().SetOrbDefaults());
      rarity = 4;
      value = 25000;
      color = new Color(185, 248, 248);
      damage = 39;
      shotsToPrimaryTarget = 2;
      maxShotsPerVolley = 3;
      randDegrees = 60;
      projectileSpeedStart = 12.5f;
      homingIncreaseRate = 0.05f;
      homingIncreaseDelay = 20;
      dustScale = 1.8f;
      lightStrength = 1.2f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb3"));
      recipe.AddIngredient(ItemID.MythrilBar, 12);
      recipe.AddIngredient(ItemID.SoulofLight, 5);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();

      recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb3"));
      recipe.AddIngredient(ItemID.OrichalcumBar, 12);
      recipe.AddIngredient(ItemID.SoulofLight, 5);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb5 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff5>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein5>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb4().SetOrbDefaults());
      rarity = 5;
      value = 50000;
      color = new Color(255, 228, 160);
      damage = 52;
      targets = 3;
      shotsPerTarget = 2;
      maxShotsPerVolley = 5;
      pierce = 2;
      homingIncreaseDelay = 17;
      targetMaxDist = 440f;
      dustScale = 2.2f;
      lightStrength = 1.4f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb4"));
      recipe.AddIngredient(ItemID.HallowedBar, 15);
      recipe.AddIngredient(ItemID.SoulofLight, 10);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb6 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff6>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein6>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb5().SetOrbDefaults());
      rarity = 8;
      value = 100000;
      color = new Color(0, 180, 174, 210);
      damage = 68;
      shotsToPrimaryTarget = 3;
      maxShotsPerVolley = 6;
      minCooldown = 10f;
      shortCooldown = 34f;
      longCooldown = 52f;
      targetThroughWallDist = 224f;
      homingIncreaseRate = 0.0625f;
      projectileSpeedStart = 14.5f;
      projectileSpeedIncreaseRate = 0.825f;
      projectileSpeedIncreaseDelay = 17;
      randDegrees = 70;
      dustScale = 2.6f;
      lightStrength = 2.25f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb5"));
      recipe.AddIngredient(ItemID.SpectreBar, 12);
      recipe.AddIngredient(ItemID.SoulofLight, 15);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb7 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff7>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein7>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb6().SetOrbDefaults());
      rarity = 9;
      value = 250000;
      color = new Color(78, 38, 102);
      damage = 84;
      targets = 4;
      shotsPerTarget = 3;
      maxShotsPerVolley = 9;
      homingIncreaseRate = 0.025f;
      projectileSpeedStart = 16f;
      targetMaxDist = 510f;
      randDegrees = 120;
      dustScale = 3f;
      lightStrength = 4.5f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb6"));
      recipe.AddIngredient(ItemID.FragmentSolar, 5);
      recipe.AddIngredient(ItemID.FragmentVortex, 5);
      recipe.AddIngredient(ItemID.FragmentNebula, 5);
      recipe.AddIngredient(ItemID.FragmentStardust, 5);
      recipe.AddIngredient(ItemID.SoulofLight, 20);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }

  public class SpiritOrb8 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff8>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein8>();

    internal override SpiritOrb SetOrbDefaults() {
      CopyFrom(new SpiritOrb7().SetOrbDefaults());
      rarity = 10;
      value = 500000;
      color = new Color(220, 220, 220);
      damage = 92;
      pierce = 1; // TODO: Revert to 3
      shotsPerBurst = 4;
      targets = 6;
      shotsToPrimaryTarget = 4;
      shotsPerTarget = 2;
      maxShotsPerVolley = 10;
      longCooldown = 55f;
      homingStrengthStart = 0.05f;
      homingIncreaseDelay = 15;
      projectileSpeedStart = 20f;
      projectileSpeedIncreaseRate = 1f;
      projectileSpeedIncreaseDelay = 35;
      randDegrees = 180;
      targetMaxDist = 650f;
      targetThroughWallDist = 370f;
      dustScale = 3.35f;
      lightStrength = 2.5f;
      return this;
    }

    public override void AddRecipes() {
      var recipe = new ModRecipe(mod);
      recipe.AddIngredient(mod.GetItem("SpiritOrb7"));
      recipe.AddIngredient(ItemID.LunarBar, 12);
      recipe.AddIngredient(ItemID.FragmentSolar, 10);
      recipe.AddIngredient(ItemID.FragmentVortex, 10);
      recipe.AddIngredient(ItemID.FragmentNebula, 10);
      recipe.AddIngredient(ItemID.FragmentStardust, 10);
      recipe.AddIngredient(ItemID.SoulofLight, 30);
      recipe.AddTile(mod.GetTile("SpiritSapling"));
      recipe.SetResult(this);
      recipe.AddRecipe();
    }
  }
}
