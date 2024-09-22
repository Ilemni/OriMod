using Terraria.ID;

namespace OriMod.Items;

public sealed class SpiritOrb1() : SpiritOrb(1) {
  public override void AddRecipes() {
    GetRecipe()
      .AddIngredient(ItemID.SilverBar, 8)
      .Register();

    GetRecipe()
      .AddIngredient(ItemID.TungstenBar, 8)
      .Register();
  }
}

public sealed class SpiritOrb2() : SpiritOrb(2) {
  public override void AddRecipes() {
    GetRecipe<SpiritOrb1>()
      .AddIngredient(ItemID.DemoniteBar, 12)
      .Register();

    GetRecipe<SpiritOrb1>()
      .AddIngredient(ItemID.CrimtaneBar, 12)
      .Register();
  }
}

public sealed class SpiritOrb3() : SpiritOrb(3) {
  public override void AddRecipes() {
    GetRecipe<SpiritOrb2>()
      .AddIngredient(ItemID.HellstoneBar, 15)
      .Register();
  }
}

public sealed class SpiritOrb4() : SpiritOrb(4) {
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

public sealed class SpiritOrb5() : SpiritOrb(5) {
  public override void AddRecipes() {
    GetRecipe<SpiritOrb4>()
      .AddIngredient(ItemID.HallowedBar, 15)
      .AddIngredient(ItemID.SoulofLight, 10)
      .Register();
  }
}

public sealed class SpiritOrb6() : SpiritOrb(6) {
  public override void AddRecipes() {
    GetRecipe<SpiritOrb5>()
      .AddIngredient(ItemID.SpectreBar, 12)
      .AddIngredient(ItemID.SoulofLight, 15)
      .Register();
  }
}

public sealed class SpiritOrb7() : SpiritOrb(7) {
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

public sealed class SpiritOrb8() : SpiritOrb(8) {
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
