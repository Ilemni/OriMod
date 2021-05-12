using System;
using System.IO;
using log4net;
using Microsoft.Xna.Framework;
using OriMod.Networking;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// The mod of this assembly.
  /// </summary>
  public sealed partial class OriMod : Mod {
    public OriMod() {
      Properties = new ModProperties {
        Autoload = true,
        AutoloadGores = true,
        AutoloadSounds = true
      };
      instance = this;
    }

    /// <summary>
    /// Singleton instance of this mod.
    /// </summary>
    public static OriMod instance;

    /// <summary>
    /// <inheritdoc cref="OriConfigClient1"/>
    /// </summary>
    public static OriConfigClient1 ConfigClient { get; internal set; }

    /// <summary>
    /// GitHub profile that the mod's repository is stored on.
    /// </summary>
    public static string GithubUserName => "TwiliChaos";

    /// <summary>
    /// Name of the GitHub repository this mod is stored on.
    /// </summary>
    public static string GithubProjectName => "OriMod";

    #region Logging Shortcuts

    internal static ILog Log => instance.Logger;

    /// <summary>
    /// Gets localized text with key <c>Mods.OriMod.<paramref name="key"/></c>.
    /// </summary>
    /// <param name="key">Key in lang file.</param>
    internal static LocalizedText GetText(string key) => Language.GetText($"Mods.OriMod.{key}");

    /// <summary>
    /// Gets localized text with key <c>Mods.OriMod.Error.<paramref name="key"/></c>.
    /// </summary>
    /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
    private static LocalizedText GetErrorText(string key) => Language.GetText($"Mods.OriMod.Error.{key}");

    /// <summary>
    /// Shows an error in chat and in the logger, with key <c>Mods.OriMod.Error.<paramref name="key"/></c>.
    /// </summary>
    /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
    /// <param name="log">Whether or not to write to logger.</param>
    internal static void Error(string key, bool log = true) => PrintError(GetErrorText(key).Value, log);

    /// <summary> Shows an error in chat and in the logger, using default localized text. Has formatting.</summary>
    /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
    /// <param name="log">Whether or not to write to logger.</param>
    /// <param name="args">Formatting args.</param>
    internal static void Error(string key, bool log = true, params object[] args) =>
      PrintError(GetErrorText(key).Format(args), log);

    /// <summary> Shows an error in chat and in the logger, using a string literal.</summary>
    /// <param name="text">String literal to print.</param>
    /// <param name="log">Whether or not to write to logger.</param>
    private static void PrintError(string text, bool log = true) {
      if (log) {
        Log.Error(text);
      }

      Main.NewText(text, Color.Red);
    }

    #endregion

    /// <summary>
    /// Key used for controlling <see cref="Abilities.SoulLink"/>.
    /// </summary>
    // ReSharper disable once UnassignedField.Global
    [Obsolete] public static ModHotKey soulLinkKey;

    /// <summary>
    /// Key used for controlling <see cref="Abilities.Bash"/>.
    /// </summary>
    public static ModHotKey bashKey;

    /// <summary>
    /// Key used for activating <see cref="Abilities.Dash"/> and <see cref="Abilities.ChargeDash"/>.
    /// </summary>
    public static ModHotKey dashKey;

    /// <summary>
    /// Key used for controlling <see cref="Abilities.Climb"/>.
    /// </summary>
    public static ModHotKey climbKey;

    /// <summary>
    /// Key used for controlling <see cref="Abilities.Glide"/>.
    /// </summary>
    public static ModHotKey featherKey;

    /// <summary>
    /// Key used for the charging of <see cref="Abilities.ChargeDash"/> and <see cref="Abilities.ChargeJump"/>.
    /// </summary>
    public static ModHotKey chargeKey;

    /// <summary>
    /// Key used for activating <see cref="Abilities.Burrow"/>.
    /// </summary>
    public static ModHotKey burrowKey;

    public override void AddRecipeGroups() {
      RecipeGroup.RegisterGroup("OriMod:EnchantedItems",
        new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Enchanted Items", ItemID.EnchantedSword,
          ItemID.EnchantedBoomerang, ItemID.Arkhalis));
      RecipeGroup.RegisterGroup("OriMod:MovementAccessories",
        new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Basic Movement Accessories", ItemID.Aglet,
          ItemID.AnkletoftheWind, ItemID.RocketBoots, ItemID.HermesBoots, ItemID.CloudinaBottle, ItemID.FlurryBoots,
          ItemID.SailfishBoots, ItemID.SandstorminaBottle, ItemID.FartinaJar, ItemID.ShinyRedBalloon, ItemID.ShoeSpikes,
          ItemID.ClimbingClaws));

      RecipeGroup.RegisterGroup("OriMod:IronBars",
        new RecipeGroup(() => "Iron/Lead Bars", ItemID.IronBar, ItemID.LeadBar));
      RecipeGroup.RegisterGroup("OriMod:GoldBars",
        new RecipeGroup(() => "Gold/Platinum Bars", ItemID.GoldBar, ItemID.PlatinumBar));
      RecipeGroup.RegisterGroup("OriMod:DarkBars",
        new RecipeGroup(() => "Demonite/Crimtane Bars", ItemID.DemoniteBar, ItemID.CrimtaneBar));
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars1",
        new RecipeGroup(() => "Cobalt/Palladium Bars", ItemID.CobaltBar, ItemID.PalladiumBar));
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars2",
        new RecipeGroup(() => "Mythril/Orichalcum Bars", ItemID.MythrilBar, ItemID.OrichalcumBar));
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars3",
        new RecipeGroup(() => "Adamantite/Titanium Bars", ItemID.AdamantiteBar, ItemID.TitaniumBar));
      RecipeGroup.RegisterGroup("OriMod:LunarFragments",
        new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Lunar Fragments", ItemID.FragmentNebula,
          ItemID.FragmentSolar, ItemID.FragmentStardust, ItemID.FragmentVortex));

      RecipeGroup.RegisterGroup("OriMod:WallJumpGear",
        new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Wall Jump Gear", ItemID.ClimbingClaws,
          ItemID.ShoeSpikes));
      RecipeGroup.RegisterGroup("OriMod:JumpBottles",
        new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Jump-Enhancing Bottles", ItemID.CloudinaBottle,
          ItemID.BlizzardinaBottle, ItemID.SandstorminaBottle, ItemID.TsunamiInABottle, ItemID.FartinaJar));
      RecipeGroup.RegisterGroup("OriMod:JumpBalloons",
        new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Jump-Enhancing Balloons",
          ItemID.CloudinaBalloon, ItemID.BlizzardinaBalloon, ItemID.SandstorminaBalloon, ItemID.SharkronBalloon,
          ItemID.FartInABalloon));
    }

    public override void Load() {
      //SoulLinkKey = RegisterHotKey("SoulLink", "E");
      bashKey = RegisterHotKey("Bash", "Mouse2");
      dashKey = RegisterHotKey("Dash", "LeftControl");
      climbKey = RegisterHotKey("Climbing", "LeftShift");
      featherKey = RegisterHotKey("Feather", "LeftShift");
      chargeKey = RegisterHotKey("Charge", "W");
      burrowKey = RegisterHotKey("Burrow", "LeftControl");
      if (!Main.dedServ) {
        AddEquipTexture(null, EquipType.Head, "OriHead", "OriMod/PlayerEffects/OriHead");
      }

      SeinData.Load();
    }

    public override void PostSetupContent() {
      FootstepManager.Initialize();
      TileCollection.Initialize();
    }

    /// <summary>
    /// Use this to null static reference types on unload.
    /// </summary>
    public static event Action OnUnload;

    public override void Unload() {
      OnUnload?.Invoke();
      OnUnload = null;

      instance = null;

      bashKey = null;
      dashKey = null;
      climbKey = null;
      featherKey = null;
      chargeKey = null;
      burrowKey = null;
      //SoulLinkKey = null;
      ConfigClient = null;
    }

    public override void HandlePacket(BinaryReader reader, int fromWho) {
      if (Main.netMode == NetmodeID.MultiplayerClient) {
        // If packet is sent TO server, it is FROM player.
        // If packet is sent TO player, it is FROM server (This block) and fromWho is 255.
        // Server-written packet includes the fromWho, the player that created it.
        // Now in either case of this being server or player, the fromWho is the player.
        fromWho = reader.ReadUInt16();
      }

      ModNetHandler.Instance.HandlePacket(reader, fromWho);
    }
  }
}