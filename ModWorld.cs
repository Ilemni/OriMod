using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod {
  public class OriWorld : ModWorld {
    public override bool Autoload(ref string name) => true;
    public static int GlobalSeinUpgrade { get; private set; }
    public static void UpdateOriPlayerSeinStates(byte upgrade) {
      if (upgrade <= GlobalSeinUpgrade) return;
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
    internal string Version;
    public override TagCompound Save() => new TagCompound {
      ["SeinUpgrade"] = GlobalSeinUpgrade,
      ["Version"] = ModLoader.GetMod("OriMod").Version.ToString(),
    };
    public override void Load(TagCompound tag) {
      GlobalSeinUpgrade = tag.GetAsInt("SeinUpgrade");
      CheckSeinUpgrade();
      Version = tag.GetString("Version");
    }
    private void CheckSeinUpgrade() {
      int oldUpgrade = GlobalSeinUpgrade;
      
      if (GlobalSeinUpgrade == 12 && !NPC.downedMoonlord) GlobalSeinUpgrade = 11;
      if (GlobalSeinUpgrade == 11 || GlobalSeinUpgrade == 10) {
        int downs = 0;
        if (NPC.downedTowerNebula) downs += 1;
        if (NPC.downedTowerSolar) downs += 1;
        if (NPC.downedTowerStardust) downs += 1;
        if (NPC.downedTowerStardust) downs += 1;
        if (downs < 4) {
          GlobalSeinUpgrade = 10;
        }
        if (downs < 2) {
          GlobalSeinUpgrade = 9;
        }
      }
      if (GlobalSeinUpgrade == 9 && !NPC.downedAncientCultist) GlobalSeinUpgrade = 8;
      if (GlobalSeinUpgrade == 8 && !NPC.downedGolemBoss) GlobalSeinUpgrade = 7;
      if (GlobalSeinUpgrade == 7 && !NPC.downedPlantBoss) GlobalSeinUpgrade = 6;
      if (GlobalSeinUpgrade == 6 && !(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)) GlobalSeinUpgrade = 5;
      if (GlobalSeinUpgrade == 5 && !(NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)) GlobalSeinUpgrade = 4;
      if (GlobalSeinUpgrade == 4 && !Main.hardMode) GlobalSeinUpgrade = 3;
      if (GlobalSeinUpgrade == 3 && !NPC.downedBoss3) GlobalSeinUpgrade = 2;
      if (GlobalSeinUpgrade == 2 && !NPC.downedBoss2) GlobalSeinUpgrade = 1;
      if (GlobalSeinUpgrade == 1 && !NPC.downedBoss1) GlobalSeinUpgrade = 0;
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