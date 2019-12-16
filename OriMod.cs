using System;
using System.IO;
// using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod {
  partial class OriMod : Mod {
    public static string GithubUserName => "TwiliChaos";
    public static string GithubProjectName => "OriMod";
    internal static bool? OwnsBlindForest { get; private set; }

    public static OriConfigClient1 ConfigClient { get; internal set; }
    public static OriConfigClient2 ConfigAbilities { get; internal set; }

    #region Logging Shortcuts

    internal static log4net.ILog Log => OriMod.Instance.Logger;

    /// <summary> Gets localiced text with key `Mods.OriMod.{key}`</summary>
    /// <param name="key">Key in lang file</param>
    internal static LocalizedText LangText(string key) => Language.GetText($"Mods.OriMod.{key}");

    /// <summary> Gets localiced text with key `Mods.OriMod.Error.{key}`</summary>
    /// <param name="key">Key in lang file, starting with `Error.`</param>
    internal static LocalizedText LangErr(string key) => Language.GetText($"Mods.OriMod.Error.{key}");

    /// <summary> Shows an error in chat and in the logger, using default localized text. </summary>
    /// <param name="key">Key in lang file</param>
    /// <param name="log">Write to logger</param>
    internal static void Error(string key, bool log = true) => ErrorText(LangErr(key).Value, log);

    /// <summary> Shows an error in chat and in the logger, using default localized text. Has formatting. </summary>
    /// <param name="key">Key in lang file</param>
    /// <param name="log">Write to logger</param>
    /// <param name="args">Formatting args</param>
    internal static void ErrorFormat(string key, bool log = true, params object[] args) => ErrorText(LangErr(key).Format(args), log);

    /// <summary> Shows an error in chat and in the logger, using a string literal. </summary>
    /// <param name="text">String literal to show</param>
    /// <param name="log">Write to logger</param>
    internal static void ErrorText(string text, bool log = true) {
      if (log) {
        Log.Error(text);
      }
      Main.NewText(text, Color.Red);
    }

    /// <summary> Write an error to the logger, using a key in the language file </summary>
    /// <param name="key"></param>
    internal static void LogError(string key) => Log.Error(LangErr(key));

    #endregion

    public static ModHotKey SoulLinkKey;
    public static ModHotKey BashKey;
    public static ModHotKey DashKey;
    public static ModHotKey ClimbKey;
    public static ModHotKey FeatherKey;
    public static ModHotKey ChargeKey;
    public static ModHotKey BurrowKey;
    public static OriMod Instance;

    public OriMod() {
      Properties = new ModProperties() {
        Autoload = true,
        AutoloadGores = true,
        AutoloadSounds = true
      };
      Instance = this;
    }

    public override void AddRecipeGroups() {
      var group1 = new RecipeGroup(() => "Any Enchanted Items", new int[] {
        ItemID.EnchantedSword,
        ItemID.EnchantedBoomerang,
        ItemID.Arkhalis
      });

      var group2 = new RecipeGroup(() => "Any Basic Movement Accessories", new int[] {
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
      });

      // Registers the new recipe group with the specified name
      RecipeGroup.RegisterGroup("OriMod:EnchantedItems", group1);
      RecipeGroup.RegisterGroup("OriMod:MovementAccessories", group2);
    }

    public override void Load() {
      SoulLinkKey = RegisterHotKey("SoulLink", "E");
      BashKey = RegisterHotKey("Bash", "Mouse2");
      DashKey = RegisterHotKey("Dash", "LeftControl");
      ClimbKey = RegisterHotKey("Climbing", "LeftShift");
      FeatherKey = RegisterHotKey("Feather", "LeftShift");
      ChargeKey = RegisterHotKey("Charge", "W");
      BurrowKey = RegisterHotKey("Burrow", "LeftControl");
      if (!Main.dedServ) {
        // Add certain equip textures
        AddEquipTexture(null, EquipType.Head, "OriHead", "OriMod/PlayerEffects/OriHead");
      }
      LoadSeinUpgrades();
      // if (OwnsBlindForest == null) {
      //   bool owned =
      //     checkInstalled(@"Software\Valve\Steam\Apps\387290") ||
      //     checkInstalled(@"Software\Valve\Steam\Apps\261570") ||
      //     checkInstalled(@"Software\GOG.com\Games\1384944984", checkValue:null);
      //   Log.Info($"Ori is owned: {owned}");
      // }
    }

    // public static bool checkInstalled(string rkey, string rvalue="Installed", string checkValue="1") {
    //   RegistryKey key = Registry.CurrentUser.OpenSubKey(rkey);
    //   if (key == null) return false;
    //   if (checkValue == null || key.GetValue(rvalue).ToString() == checkValue) {
    //     key.Close();
    //     return true;
    //   }
    //   key.Close();
    //   return false;
    // }

    public override void Unload() {
      BashKey = null;
      DashKey = null;
      ClimbKey = null;
      FeatherKey = null;
      ChargeKey = null;
      BurrowKey = null;
      SoulLinkKey = null;
      SeinUpgrades = null;
      Instance = null;
      ConfigClient = null;
      ConfigAbilities = null;

      // Unload ModPlayer
      try {
        for (int p = 0, len = Main.player.Length; p < len; p++) {
          var player = Main.player[p];
          OriPlayer oPlayer = null;
          try {
            oPlayer = player.GetModPlayer<OriPlayer>();
          }
          catch { // All OriPlayers unloaded
            break;
          }
          oPlayer?.Unload();
        }
      }
      catch (Exception ex) {
        Log.Error($"Error while unloading OriPlayers, unload for OriPlayers cancelled.\n{ex}");
      }
    }

    public override void HandlePacket(BinaryReader reader, int fromWho)
      => ModNetHandler.HandlePacket(reader, fromWho);

    public override object Call(params object[] args) {
      int len = args.Length;
      if (len > 0 && args[0] is string cmd) {
        switch (cmd) {
          case "ResetPlayerModData": {
              if (len >= 2) {
                OriPlayer oPlayer;
                object obj = args[1];
                if (obj is Player player) {
                  oPlayer = player.GetModPlayer<OriPlayer>();
                }
                else if (obj is ModPlayer modPlayer) {
                  oPlayer = modPlayer.player.GetModPlayer<OriPlayer>();
                }
                else {
                  Log.Warn($"{this.Name}.Call() - ResetPlayerModData - Expected type Player, got {obj.GetType()}");
                  return false;
                }
                oPlayer.ResetData();
                return true;
              }
              break;
            }
        }
      }
      return null;
    }
  }
}
