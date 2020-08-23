using System;
using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Networking;
using OriMod.UI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace OriMod {
  public sealed class OriMod : Mod {
    public OriMod() {
      Properties = new ModProperties() {
        Autoload = true,
        AutoloadGores = true,
        AutoloadSounds = true
      };
      Instance = this;
    }
    public static OriMod Instance;

    public static OriConfigClient1 ConfigClient { get; internal set; }
    public static OriConfigClient2 ConfigAbilities { get; internal set; }

    public static string GithubUserName => "TwiliChaos";
    public static string GithubProjectName => "OriMod";

    internal static UserInterface Interface;
    internal static UpgradeUI upgradeUI;

    #region Logging Shortcuts

    internal static log4net.ILog Log => Instance.Logger;

    /// <summary> Gets localiced text with key `Mods.OriMod.{key}`</summary>
    /// <param name="key">Key in lang file</param>
    internal static LocalizedText GetText(string key) => Language.GetText($"Mods.OriMod.{key}");

    /// <summary> Gets localiced text with key `Mods.OriMod.Error.{key}`</summary>
    /// <param name="key">Key in lang file, starting with `Error.`</param>
    internal static LocalizedText GetErrorText(string key) => Language.GetText($"Mods.OriMod.Error.{key}");

    /// <summary> Shows an error in chat and in the logger, using default localized text. </summary>
    /// <param name="key">Key in lang file</param>
    /// <param name="log">Write to logger</param>
    internal static void Error(string key, bool log = true) => PrintError(GetErrorText(key).Value, log);

    /// <summary> Shows an error in chat and in the logger, using default localized text. Has formatting. </summary>
    /// <param name="key">Key in lang file</param>
    /// <param name="log">Write to logger</param>
    /// <param name="args">Formatting args</param>
    internal static void Error(string key, bool log = true, params object[] args) => PrintError(GetErrorText(key).Format(args), log);

    /// <summary> Shows an error in chat and in the logger, using a string literal. </summary>
    /// <param name="text">String literal to show</param>
    /// <param name="log">Write to logger</param>
    internal static void PrintError(string text, bool log = true) {
      if (log) {
        Log.Error(text);
      }
      Main.NewText(text, Color.Red);
    }
    #endregion

    public static ModHotKey SoulLinkKey;
    public static ModHotKey BashKey;
    public static ModHotKey DashKey;
    public static ModHotKey ClimbKey;
    public static ModHotKey FeatherKey;
    public static ModHotKey ChargeKey;
    public static ModHotKey BurrowKey;

    private GameTime _lastUpdateUiGameTime;

    private bool uiShown = false;

    internal void ShowUpgradeUI() => Interface?.SetState(upgradeUI);

    internal void HideUI() => Interface?.SetState(null);
    
    public override void UpdateUI(GameTime gameTime) {
      // Temporary, for debug only
      /*if (SoulLinkKey.JustPressed) {
        uiShown ^= true;
        if (uiShown) {
          ShowUpgradeUI();
        }
        else {
          HideUI();
        }
      }*/

      _lastUpdateUiGameTime = gameTime;
      if (Interface?.CurrentState != null) {
        Interface.Update(gameTime);
      }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
      int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
      if (mouseTextIndex != -1) {
        layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("OriMod: UpgradeInterface", delegate {
            if (_lastUpdateUiGameTime != null && Interface?.CurrentState != null) {
              Interface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
            }
            return true;
        }, InterfaceScaleType.UI));
      }
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
      //SoulLinkKey = RegisterHotKey("SoulLink", "E");
      BashKey = RegisterHotKey("Bash", "Mouse2");
      DashKey = RegisterHotKey("Dash", "LeftControl");
      ClimbKey = RegisterHotKey("Climbing", "LeftShift");
      FeatherKey = RegisterHotKey("Feather", "LeftShift");
      ChargeKey = RegisterHotKey("Charge", "W");
      BurrowKey = RegisterHotKey("Burrow", "LeftControl");
      if (!Main.dedServ) {
        AddEquipTexture(null, EquipType.Head, "OriHead", "OriMod/PlayerEffects/OriHead");
        Interface = new UserInterface();
        upgradeUI = new UpgradeUI();
        upgradeUI.Activate();
      }

      SeinData.Load();
    }

    public override void PostSetupContent() {
      FootstepManager.Initialize();
      TileCollection.Initialize();
    }

    /// <summary>
    /// Use this to set static reference types to null on unload.
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

      OnUnload();
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
      ModNetHandler.Instance.HandlePacket(reader, (ushort)fromWho);
    }

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
              break;
            }
        }
      }
      return null;
    }
  }
}
