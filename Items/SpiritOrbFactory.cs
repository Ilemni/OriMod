using OriMod.Buffs;
using OriMod.Projectiles.Minions;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items; 

public class SpiritOrb1 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff1>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein1>();
    protected override int SeinType => 1;

    public override void AddRecipes() {
        GetRecipe()
            .AddIngredient(ItemID.SilverBar, 8)
            .Register();
      
        GetRecipe()
            .AddIngredient(ItemID.TungstenBar, 8)
            .Register();
    }
}

public class SpiritOrb2 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff2>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein2>();
    protected override int SeinType => 2;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb1>()
            .AddIngredient(ItemID.DemoniteBar, 12)
            .Register();

        GetRecipe<SpiritOrb1>()
            .AddIngredient(ItemID.CrimtaneBar, 12)
            .Register();
    }
}

public class SpiritOrb3 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff3>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein3>();
    protected override int SeinType => 3;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb2>()
            .AddIngredient(ItemID.HellstoneBar, 15)
            .Register();
    }
}

public class SpiritOrb4 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff4>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein4>();
    protected override int SeinType => 4;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb3>()
            .AddIngredient(ItemID.MythrilBar, 12)
            .AddIngredient(ItemID.SoulofLight, 5)
            .Register();

        GetRecipe<SpiritOrb3>()
            .AddIngredient(ItemID.OrichalcumBar, 12)
            .AddIngredient(ItemID.SoulofLight, 5)
            .Register();
    }
}

public class SpiritOrb5 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff5>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein5>();
    protected override int SeinType => 5;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb4>()
            .AddIngredient(ItemID.HallowedBar, 15)
            .AddIngredient(ItemID.SoulofLight, 10)
            .Register();
    }
}

public class SpiritOrb6 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff6>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein6>();
    protected override int SeinType => 6;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb5>()
            .AddIngredient(ItemID.SpectreBar, 12)
            .AddIngredient(ItemID.SoulofLight, 15)
            .Register();
    }
}

public class SpiritOrb7 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff7>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein7>();
    protected override int SeinType => 7;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb6>()
            .AddIngredient(ItemID.FragmentSolar, 5)
            .AddIngredient(ItemID.FragmentVortex, 5)
            .AddIngredient(ItemID.FragmentNebula, 5)
            .AddIngredient(ItemID.FragmentStardust, 5)
            .AddIngredient(ItemID.SoulofLight, 20)
            .Register();
    }
}

public class SpiritOrb8 : SpiritOrb {
    protected override int GetBuffType() => ModContent.BuffType<SeinBuff8>();
    protected override int GetShootType() => ModContent.ProjectileType<Sein8>();
    protected override int SeinType => 8;

    public override void AddRecipes() {
        GetRecipe<SpiritOrb7>()
            .AddIngredient(ItemID.LunarBar, 12)
            .AddIngredient(ItemID.FragmentSolar, 10)
            .AddIngredient(ItemID.FragmentVortex, 10)
            .AddIngredient(ItemID.FragmentNebula, 10)
            .AddIngredient(ItemID.FragmentStardust, 10)
            .AddIngredient(ItemID.SoulofLight, 30)
            .Register();
    }
}
