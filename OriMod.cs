using System.IO;
// using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  partial class OriMod : Mod {
    public static string GithubUserName => "TwiliChaos";
    public static string GithubProjectName => "OriMod";
    internal static bool? OwnsBlindForest { get; private set; }

    public static OriConfigClient1 ConfigClient { get; internal set; }
    public static OriConfigClient2 ConfigAbilities { get; internal set; }
    
    internal static log4net.ILog Log => OriMod.Instance.Logger;

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
      // Creates a new recipe group
      RecipeGroup group1 = new RecipeGroup(() => "Any Enchanted Items", new int[] {
        ItemID.EnchantedSword,
        ItemID.EnchantedBoomerang,
        ItemID.Arkhalis
      });
      RecipeGroup group2 = new RecipeGroup(() => "Any Basic Movement Accessories", new int[] {
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
      SoulLinkKey = RegisterHotKey("Soul Link", "E");
      BashKey = RegisterHotKey("Bash", "Mouse2");
      DashKey = RegisterHotKey("Dash", "LeftControl");
      ClimbKey = RegisterHotKey("Climbing", "LeftShift");
      FeatherKey = RegisterHotKey("Feather", "LeftShift");
      ChargeKey = RegisterHotKey("Charge", "W");
      BurrowKey = RegisterHotKey("Burrow", "LeftShift");
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
    }
    public override void HandlePacket(BinaryReader reader, int fromWho) {
      ModNetHandler.HandlePacket(reader, fromWho);
    }
    public override object Call(params object[] args) {
      if (args.Length == 2) {
        string cmd = args[0] as string;
        if (cmd == "ResetPlayerModData") {
          Player player = args[1] as Player;
          if (player != null) {
            OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
            oPlayer.ResetData();
          }
          else {
            Log.Warn(this.Name + ".Call() - ResetPlayerModData - Invalid player");
          }
        }
      }
      return null;
    }
  }
  internal class ModNetHandler {
    internal const byte OriState = 1;
    internal const byte Ability = 2;
    internal static OriPlayerPacketHandler oriPlayerHandler = new OriPlayerPacketHandler(OriState);
    internal static AbilityPacketHandler abilityPacketHandler = new AbilityPacketHandler(Ability);
    internal static void HandlePacket(BinaryReader r, int fromWho) {
      byte packetClass = r.ReadByte();
      switch (packetClass) {
        case OriState:
          oriPlayerHandler.HandlePacket(r, fromWho);
          break;
        case Ability:
          abilityPacketHandler.HandlePacket(r, fromWho);
          break;
        default:
          Main.NewText("Unknown Packet " + packetClass, Color.Red);
          OriMod.Log.Warn("Unknown Packet " + packetClass);
          break;
      }
    }
  }
}
