using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod;

[UsedImplicitly]
public sealed class OriRecipeGroups : ModSystem {
  public static RecipeGroup EnchantedItems { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup MovementAccessories { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup IronBars { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup GoldBars { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup DarkBars { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup HardmodeBars1 { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup HardmodeBars2 { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup HardmodeBars3 { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup LunarFragments { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup WallJumpGear { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup JumpBottles { get; private set; } = null!; // AddRecipeGroup()
  public static RecipeGroup JumpBalloons { get; private set; } = null!; // AddRecipeGroup()

  public override void AddRecipeGroups() {
    EnchantedItems = new RecipeGroup(EnchantedItemsText,
      ItemID.EnchantedSword, ItemID.EnchantedBoomerang, ItemID.Terragrim);
    MovementAccessories = new RecipeGroup(MovementAccessoriesText,
      ItemID.Aglet, ItemID.AnkletoftheWind, ItemID.RocketBoots, ItemID.HermesBoots, ItemID.CloudinaBottle,
      ItemID.FlurryBoots, ItemID.SailfishBoots, ItemID.SandstorminaBottle, ItemID.FartinaJar, ItemID.ShinyRedBalloon,
      ItemID.ShoeSpikes, ItemID.ClimbingClaws, ItemID.EoCShield, ItemID.BlizzardinaBottle, ItemID.TsunamiInABottle);
    IronBars = new RecipeGroup(IronBarsText,
      ItemID.IronBar, ItemID.LeadBar);
    GoldBars = new RecipeGroup(GoldBarsText,
      ItemID.GoldBar, ItemID.PlatinumBar);
    DarkBars = new RecipeGroup(DarkBarsText,
      ItemID.DemoniteBar, ItemID.CrimtaneBar);
    HardmodeBars1 = new RecipeGroup(HardmodeBars1Text,
      ItemID.CobaltBar, ItemID.PalladiumBar);
    HardmodeBars2 = new RecipeGroup(HardmodeBars2Text,
      ItemID.MythrilBar, ItemID.OrichalcumBar);
    HardmodeBars3 = new RecipeGroup(HardmodeBars3Text,
      ItemID.AdamantiteBar, ItemID.TitaniumBar);
    LunarFragments = new RecipeGroup(LunarFragmentsText,
      ItemID.FragmentNebula, ItemID.FragmentSolar, ItemID.FragmentStardust, ItemID.FragmentVortex);
    WallJumpGear = new RecipeGroup(WallJumpGearText,
      ItemID.ClimbingClaws, ItemID.ShoeSpikes);
    JumpBottles = new RecipeGroup(JumpBottlesText,
      ItemID.CloudinaBottle, ItemID.BlizzardinaBottle, ItemID.SandstorminaBottle,
      ItemID.TsunamiInABottle, ItemID.FartinaJar);
    JumpBalloons = new RecipeGroup(JumpBalloonsText,
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

  // Avoid lambdas in function for full mod unload
  private const string Prefix = "Mods.OriMod.RecipeGroups.";
  private string EnchantedItemsText() => Language.GetTextValue(Prefix + "EnchantedItems");
  private string MovementAccessoriesText() => Language.GetTextValue(Prefix + "MovementAccessories");
  private string IronBarsText() => Language.GetTextValue(Prefix + "IronBars");
  private string GoldBarsText() => Language.GetTextValue(Prefix + "GoldBars");
  private string DarkBarsText() => Language.GetTextValue(Prefix + "DarkBars");
  private string HardmodeBars1Text() => Language.GetTextValue(Prefix + "HardmodeBars1");
  private string HardmodeBars2Text() => Language.GetTextValue(Prefix + "HardmodeBars2");
  private string HardmodeBars3Text() => Language.GetTextValue(Prefix + "HardmodeBars3");
  private string LunarFragmentsText() => Language.GetTextValue(Prefix + "LunarFragments");
  private string WallJumpGearText() => Language.GetTextValue(Prefix + "WallJumpGear");
  private string JumpBottlesText() => Language.GetTextValue(Prefix + "JumpBottles");
  private string JumpBalloonsText() => Language.GetTextValue(Prefix + "JumpBalloons");

  public override void Unload() {
    EnchantedItems = null!;
    MovementAccessories = null!;
    IronBars = null!;
    GoldBars = null!;
    DarkBars = null!;
    HardmodeBars1 = null!;
    HardmodeBars2 = null!;
    HardmodeBars3 = null!;
    LunarFragments = null!;
    WallJumpGear = null!;
    JumpBottles = null!;
    JumpBalloons = null!;
  }
}
