﻿using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod {
  public class OriRecipeGroups : ModSystem {
    public static RecipeGroup EnchantedItems { get; private set; }
    public static RecipeGroup MovementAccessories { get; private set; }
    public static RecipeGroup IronBars { get; private set; }
    public static RecipeGroup GoldBars { get; private set; }
    public static RecipeGroup DarkBars { get; private set; }
    public static RecipeGroup HardmodeBars1 { get; private set; }
    public static RecipeGroup HardmodeBars2 { get; private set; }
    public static RecipeGroup HardmodeBars3 { get; private set; }
    public static RecipeGroup LunarFragments { get; private set; }
    public static RecipeGroup WallJumpGear { get; private set; }
    public static RecipeGroup JumpBottles { get; private set; }
    public static RecipeGroup JumpBalloons { get; private set; }

    public override void AddRecipeGroups() {
      EnchantedItems = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Enchanted Items",
        ItemID.EnchantedSword, ItemID.EnchantedBoomerang, ItemID.Arkhalis, ItemID.Terragrim);
      MovementAccessories = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Basic Movement Accessories",
        ItemID.Aglet, ItemID.AnkletoftheWind, ItemID.RocketBoots, ItemID.HermesBoots, ItemID.CloudinaBottle,
        ItemID.FlurryBoots, ItemID.SailfishBoots, ItemID.SandstorminaBottle, ItemID.FartinaJar, ItemID.ShinyRedBalloon,
        ItemID.ShoeSpikes, ItemID.ClimbingClaws, ItemID.EoCShield, ItemID.BlizzardinaBottle, ItemID.TsunamiInABottle);
      IronBars = new RecipeGroup(() => "Iron/Lead Bars", ItemID.IronBar, ItemID.LeadBar);
      GoldBars = new RecipeGroup(() => "Gold/Platinum Bars", ItemID.GoldBar, ItemID.PlatinumBar);
      DarkBars = new RecipeGroup(() => "Demonite/Crimtane Bars", ItemID.DemoniteBar, ItemID.CrimtaneBar);
      HardmodeBars1 = new RecipeGroup(() => "Cobalt/Palladium Bars", ItemID.CobaltBar, ItemID.PalladiumBar);
      HardmodeBars2 = new RecipeGroup(() => "Mythril/Orichalcum Bars", ItemID.MythrilBar, ItemID.OrichalcumBar);
      HardmodeBars3 = new RecipeGroup(() => "Adamantite/Titanium Bars", ItemID.AdamantiteBar, ItemID.TitaniumBar);
      LunarFragments = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Lunar Fragments",
        ItemID.FragmentNebula, ItemID.FragmentSolar, ItemID.FragmentStardust, ItemID.FragmentVortex);
      WallJumpGear = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Wall Jump Gear",
        ItemID.ClimbingClaws, ItemID.ShoeSpikes);
      JumpBottles = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Jump-Enhancing Bottles",
        ItemID.CloudinaBottle, ItemID.BlizzardinaBottle, ItemID.SandstorminaBottle,
        ItemID.TsunamiInABottle, ItemID.FartinaJar);
      JumpBalloons = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Jump-Enhancing Balloons",
        ItemID.CloudinaBalloon, ItemID.BlizzardinaBalloon, ItemID.SandstorminaBalloon, ItemID.SharkronBalloon,
        ItemID.FartInABalloon);

      RecipeGroup.RegisterGroup("OriMod:EnchantedItems", EnchantedItems);
      RecipeGroup.RegisterGroup("OriMod:MovementAccessories", MovementAccessories);
      RecipeGroup.RegisterGroup("OriMod:IronBars", IronBars);
      RecipeGroup.RegisterGroup("OriMod:GoldBars", GoldBars);
      RecipeGroup.RegisterGroup("OriMod:DarkBars", DarkBars);
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars1", HardmodeBars1);
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars2", HardmodeBars2);
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars3", HardmodeBars3);
      RecipeGroup.RegisterGroup("OriMod:LunarFragments", LunarFragments);
      RecipeGroup.RegisterGroup("OriMod:WallJumpGear", WallJumpGear);
      RecipeGroup.RegisterGroup("OriMod:JumpBottles", JumpBottles);
      RecipeGroup.RegisterGroup("OriMod:JumpBalloons", JumpBalloons);
    }

    public override void Unload() {
      EnchantedItems = null;
      MovementAccessories = null;
      IronBars = null;
      GoldBars = null;
      DarkBars = null;
      HardmodeBars1 = null;
      HardmodeBars2 = null;
      HardmodeBars3 = null;
      LunarFragments = null;
      WallJumpGear = null;
      JumpBottles = null;
      JumpBalloons = null;
    }
  }
}
