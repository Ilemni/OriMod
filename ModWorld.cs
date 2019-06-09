using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod {
  public class OriWorld : ModWorld {
    public override bool Autoload(ref string name) => true;
    private static byte _globalSeinUpgrade = 0;
    public static byte GlobalSeinUpgrade {
      get {
        return _globalSeinUpgrade;
      }
      internal set {
        if (value < _globalSeinUpgrade && doResetSein) {
          _globalSeinUpgrade = value;
          doResetSein = false;
        }
        else if (value > _globalSeinUpgrade) {
          _globalSeinUpgrade = value;
        }
      }
    }
    private static bool doResetSein = false;
    internal static void RegressSeinUpgrade(int u=0) {
      doResetSein = true;
      GlobalSeinUpgrade = (byte)u;
    }
    public static void UpdateOriPlayerSeinStates(byte upgrade) {
      // Main.NewText("Upgrade from " + GlobalSeinUpgrade + " to " + upgrade);
      GlobalSeinUpgrade = upgrade;
      // foreach(Player p in Main.player) {
      //   if (!p.active) continue;
      //   OriPlayer oPlayer = p.GetModPlayer<OriPlayer>();
      //   oPlayer.SeinMinionUpgrade = GlobalSeinUpgrade;
      // }
      LocalizedText text = Language.GetText("Mods.OriMod.SeinUpgrade.Upgraded" + GlobalSeinUpgrade);
      if (Main.netMode == 0) {
        // Main.NewText(text.ToString(), Color.White);
      }
      else if (Main.netMode == 2) {
        // NetMessage.BroadcastChatMessage(text.ToNetworkText(), Color.White);
      }
    }
    public override TagCompound Save() {
      byte seinUpgrade = GlobalSeinUpgrade;
      return new TagCompound {
        { "SeinUpgrade", seinUpgrade }
      };
    }
    public override void Load(TagCompound tag) {
      GlobalSeinUpgrade = tag.GetByte("SeinUpgrade");
      byte oldUpgrade = GlobalSeinUpgrade;
      
      if (GlobalSeinUpgrade == 12 && !NPC.downedMoonlord) RegressSeinUpgrade(11);
      if (GlobalSeinUpgrade == 11 && !(NPC.downedTowerNebula && NPC.downedTowerSolar && NPC.downedTowerStardust && NPC.downedTowerStardust)) RegressSeinUpgrade(10);
      if (GlobalSeinUpgrade == 10) {
        byte downs = 0;
        if (NPC.downedTowerNebula) downs += 1;
        if (NPC.downedTowerSolar) downs += 1;
        if (NPC.downedTowerStardust) downs += 1;
        if (NPC.downedTowerStardust) downs += 1;
        if (downs < 2) {
          RegressSeinUpgrade(9);
        }
      }
      if (GlobalSeinUpgrade == 9 && !NPC.downedAncientCultist) RegressSeinUpgrade(8);
      if (GlobalSeinUpgrade == 8 && !NPC.downedGolemBoss) RegressSeinUpgrade(7);
      if (GlobalSeinUpgrade == 7 && !NPC.downedPlantBoss) RegressSeinUpgrade(6);
      if (GlobalSeinUpgrade == 6 && !(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)) RegressSeinUpgrade(5);
      if (GlobalSeinUpgrade == 5 && !(NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)) RegressSeinUpgrade(4);
      if (GlobalSeinUpgrade == 4 && !Main.hardMode) RegressSeinUpgrade(3);
      if (GlobalSeinUpgrade == 3 && !NPC.downedBoss3) RegressSeinUpgrade(2);
      if (GlobalSeinUpgrade == 2 && !NPC.downedBoss2) RegressSeinUpgrade(1);
      if (GlobalSeinUpgrade == 1 && !NPC.downedBoss1) RegressSeinUpgrade(0);
      if (GlobalSeinUpgrade != oldUpgrade) {
        // string text = "Due to the world being modified, Sein's upgrade regressed from " + oldUpgrade + " to " + GlobalSeinUpgrade;
        // Main.NewText(text);
        // ErrorLogger.Log(text);
      }
    }
    public override void NetSend(BinaryWriter writer) {
      writer.Write(GlobalSeinUpgrade);
    }
    public override void NetReceive(BinaryReader reader) {
      GlobalSeinUpgrade = reader.ReadByte();
    }
  }
}