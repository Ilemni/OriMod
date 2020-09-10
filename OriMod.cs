using System;
using System.IO;
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
  public sealed class OriMod : Mod {
    public OriMod() {
      Properties = new ModProperties() {
        Autoload = true,
        AutoloadGores = true,
        AutoloadSounds = true
      };
      Instance = this;
    }

    /// <summary>
    /// Singleton instance of this mod.
    /// </summary>
    public static OriMod Instance;

    /// <summary>
    /// <inheritdoc cref="OriConfigClient1"/>
    /// </summary>
    public static OriConfigClient1 ConfigClient { get; internal set; }

    /// <summary>
    /// <inheritdoc cref="OriConfigClient2"/>
    /// </summary>
    public static OriConfigClient2 ConfigAbilities { get; internal set; }

    /// <summary>
    /// GitHub profile that the mod's repository is stored on.
    /// </summary>
    public static string GithubUserName => "TwiliChaos";

    /// <summary>
    /// Name of the GitHub repository this mod is stored on.
    /// </summary>
    public static string GithubProjectName => "OriMod";

    #region Logging Shortcuts

    internal static log4net.ILog Log => Instance.Logger;

    /// <summary>
    /// Gets localiced text with key <c>Mods.OriMod.<paramref name="key"/></c>.
    /// </summary>
    /// <param name="key">Key in lang file.</param>
    internal static LocalizedText GetText(string key) => Language.GetText($"Mods.OriMod.{key}");

    /// <summary>
    /// Gets localiced text with key <c>Mods.OriMod.Error.<paramref name="key"/></c>.
    /// </summary>
    /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
    internal static LocalizedText GetErrorText(string key) => Language.GetText($"Mods.OriMod.Error.{key}");

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
    internal static void Error(string key, bool log = true, params object[] args) => PrintError(GetErrorText(key).Format(args), log);

    /// <summary> Shows an error in chat and in the logger, using a string literal.</summary>
    /// <param name="text">String literal to print.</param>
    /// <param name="log">Whether or not to write to logger.</param>
    internal static void PrintError(string text, bool log = true) {
      if (log) {
        Log.Error(text);
      }
      Main.NewText(text, Color.Red);
    }
    #endregion

    /// <summary>
    /// Key used for controlling <see cref="Abilities.SoulLink"/>.
    /// </summary>
    [Obsolete] public static ModHotKey SoulLinkKey; // Unused
    /// <summary>
    /// Key used for controlling <see cref="Abilities.Bash"/>.
    /// </summary>
    public static ModHotKey BashKey;
    /// <summary>
    /// Key used for activating <see cref="Abilities.Dash"/> and <see cref="Abilities.ChargeDash"/>.
    /// </summary>
    public static ModHotKey DashKey;
    /// <summary>
    /// Key used for controlling <see cref="Abilities.Climb"/>.
    /// </summary>
    public static ModHotKey ClimbKey;
    /// <summary>
    /// Key used for controlling <see cref="Abilities.Glide"/>.
    /// </summary>
    public static ModHotKey FeatherKey;
    /// <summary>
    /// Key used for the charging of <see cref="Abilities.ChargeDash"/> and <see cref="Abilities.ChargeJump"/>.
    /// </summary>
    public static ModHotKey ChargeKey;
    /// <summary>
    /// Key used for activating <see cref="Abilities.Burrow"/>.
    /// </summary>
    public static ModHotKey BurrowKey;

    public override void AddRecipeGroups() {
      RecipeGroup.RegisterGroup("OriMod:EnchantedItems", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Enchanted Items", new int[] {
        ItemID.EnchantedSword,
        ItemID.EnchantedBoomerang,
        ItemID.Arkhalis
      }));
      RecipeGroup.RegisterGroup("OriMod:MovementAccessories", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Basic Movement Accessories", new int[] {
        ItemID.Aglet,
        ItemID.AnkletoftheWind,
        ItemID.RocketBoots,
        ItemID.HermesBoots,
        ItemID.CloudinaBottle,
        ItemID.FlurryBoots,
        ItemID.SailfishBoots,
        ItemID.SandstorminaBottle,
        ItemID.FartinaJar,
        ItemID.ShinyRedBalloon,
        ItemID.ShoeSpikes,
        ItemID.ClimbingClaws
      }));

      RecipeGroup.RegisterGroup("OriMod:GoldBars", new RecipeGroup(() => "Gold/Platinum Bars", new int[] { ItemID.GoldBar, ItemID.PlatinumBar }));
      RecipeGroup.RegisterGroup("OriMod:DarkBars", new RecipeGroup(() => "Demonite/Crimtane Bars", new int[] { ItemID.DemoniteBar, ItemID.CrimtaneBar }));
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars1", new RecipeGroup(() => "Cobalt/Palladium Bars", new int[] { ItemID.CobaltBar, ItemID.PalladiumBar }));
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars2", new RecipeGroup(() => "Mythril/Orichalcum Bars", new int[] { ItemID.MythrilBar, ItemID.OrichalcumBar }));
      RecipeGroup.RegisterGroup("OriMod:HardmodeBars3", new RecipeGroup(() => "Adamantite/Titanium Bars", new int[] { ItemID.AdamantiteBar, ItemID.TitaniumBar }));
      RecipeGroup.RegisterGroup("OriMod:LunarFragments", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Lunar Fragments", new int[] { ItemID.FragmentNebula, ItemID.FragmentSolar, ItemID.FragmentStardust, ItemID.FragmentVortex }));

      RecipeGroup.RegisterGroup("OriMod:WallJumpGear", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Wall Jump Gear", new int[] { ItemID.ClimbingClaws, ItemID.ShoeSpikes }));
      RecipeGroup.RegisterGroup("OriMod:JumpBottles", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Jump-Enhancing Bottles", new int[] { ItemID.CloudinaBottle, ItemID.BlizzardinaBottle, ItemID.SandstorminaBottle, ItemID.TsunamiInABottle, ItemID.FartinaJar }));
      RecipeGroup.RegisterGroup("OriMod:JumpBalloons", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Jump-Enhancing Balloons", new int[] { ItemID.CloudinaBalloon, ItemID.BlizzardinaBalloon, ItemID.SandstorminaBalloon, ItemID.SharkronBalloon, ItemID.FartInABalloon }));
    }

    public override void Load() {
      //SoulLinkKey = RegisterHotKey("SoulLink", "E");
      BashKey = RegisterHotKey("Bash", "Mouse2");
      DashKey = RegisterHotKey("Dash", "LeftControl");
      ClimbKey = RegisterHotKey("Climbing", "LeftShift");
      FeatherKey = RegisterHotKey("Feather", "LeftShift");
      ChargeKey = RegisterHotKey("Charge", "W");
      BurrowKey = RegisterHotKey("Burrow", "LeftControl");
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
      Instance = null;

      BashKey = null;
      DashKey = null;
      ClimbKey = null;
      FeatherKey = null;
      ChargeKey = null;
      BurrowKey = null;
      //SoulLinkKey = null;
      ConfigClient = null;
      ConfigAbilities = null;

      OnUnload?.Invoke();
      OnUnload = null;
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

    /// <summary>
    /// Interact with <see cref="OriMod"/> using various inputs.
    /// <list type="table">
    /// <listheader>
    /// <term>Command/Parameters</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>"ResetPlayerModData", <see cref="Player"/> -or- <see cref="ModPlayer"/></term>
    /// <description>
    /// Resets the <see cref="OriPlayer"/> data on the given <see cref="Player"/>/<see cref="ModPlayer"/> —
    /// Returns <see langword="true"/> if arguments are valid; otherwise, <see langword="false"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public override object Call(params object[] args) {
      int len = args.Length;
      if (len > 0 && args[0] is string cmd) {
        switch (cmd) {
          case "ResetPlayerModData": {
              if (len >= 2) {
                object obj = args[1];
                Player player =
                  obj is Player p ? p :
                  obj is ModPlayer modPlayer ? modPlayer.player : null;
                if (player is null) {
                  Log.Warn($"{this.Name}.Call() - ResetPlayerModData - Expected type {typeof(Player)}, got {obj.GetType()}");
                  return false;
                }
                player.GetModPlayer<OriPlayer>().ResetData();
                return true;
              }
              return false;
            }
        }
      }
      return null;
    }
  }
}
