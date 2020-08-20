using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace OriMod {
  public class OriWorld : ModWorld {
    public enum Upgrade : byte {
      None = 0,
      DefeatedEyeOfCthuhlu = 1,
      DefeatedDarkBoss = 2,
      DefeatedSkeletron = 3,
      InHardMode = 4,
      DefeatedAnyMechs = 5,
      DefeatedAllMechs = 6,
      DefeatedPlantera = 7,
      DefeatedGolem = 8,
      DefeatedLunarCultist = 9,
      DefeatedTwoPillars = 10,
      DefeatedAllPillars = 11,
      DefeatedMoonLord = 12
    }

    public static Upgrade GlobalUpgrade { get; private set; }

    public string Version;
    
    public override bool Autoload(ref string name) => true;

    public static void UpdateOriPlayerSeinStates(byte upgrade) {
      if (upgrade <= (byte)GlobalUpgrade) {
        return;
      }
      // Main.NewText("Upgrade from " + GlobalSeinUpgrade + " to " + upgrade);
      GlobalUpgrade = (Upgrade)upgrade;
      // foreach(Player p in Main.player) {
      //   if (!p.active) continue;
      //   OriPlayer oPlayer = p.GetModPlayer<OriPlayer>();
      //   oPlayer.SeinMinionUpgrade = GlobalSeinUpgrade;
      // }
      LocalizedText text = OriMod.GetText($"SeinUpgrade.Upgraded{GlobalUpgrade}");
      if (Main.netMode == NetmodeID.SinglePlayer) {
        // Main.NewText(text.ToString(), Color.White);
      }
      else if (Main.netMode == NetmodeID.Server) {
        // NetMessage.BroadcastChatMessage(text.ToNetworkText(), Color.White);
      }
    }

    public override TagCompound Save() {
      ValidateSeinUpgrade();
      return new TagCompound {
        ["SeinUpgrade"] = GlobalUpgrade,
        ["Version"] = OriMod.Instance.Version.ToString(),
    };
    }

    public override void Load(TagCompound tag) {
      GlobalUpgrade = (Upgrade)tag.GetAsInt("SeinUpgrade");
      ValidateSeinUpgrade();
      Version = tag.GetString("Version");
    }

    private void ValidateSeinUpgrade() {
      Upgrade oldUpgrade = GlobalUpgrade;

      if (GlobalUpgrade == Upgrade.DefeatedMoonLord && !NPC.downedMoonlord) {
        GlobalUpgrade = Upgrade.DefeatedAllPillars;
      }

      if (GlobalUpgrade == Upgrade.DefeatedAllPillars && !NPC.downedTowers) {
        GlobalUpgrade = Upgrade.DefeatedTwoPillars;
      }

      if (GlobalUpgrade == Upgrade.DefeatedTwoPillars) {
        int downs = 0;
        if (NPC.downedTowerNebula) {
          downs++;
        }
        if (NPC.downedTowerSolar) {
          downs++;
        }
        if (NPC.downedTowerStardust) {
          downs++;
        }
        if (NPC.downedTowerStardust) {
          downs++;
        }
        if (downs < 2) {
          GlobalUpgrade = Upgrade.DefeatedLunarCultist;
        }
      }

      if (GlobalUpgrade == Upgrade.DefeatedLunarCultist && !NPC.downedAncientCultist) {
        GlobalUpgrade = Upgrade.DefeatedGolem;
      }

      if (GlobalUpgrade == Upgrade.DefeatedGolem && !NPC.downedGolemBoss) {
        GlobalUpgrade = Upgrade.DefeatedPlantera;
      }

      if (GlobalUpgrade == Upgrade.DefeatedPlantera && !NPC.downedPlantBoss) {
        GlobalUpgrade = Upgrade.DefeatedAllMechs;
      }

      if (GlobalUpgrade == Upgrade.DefeatedAllMechs && !(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)) {
        GlobalUpgrade = Upgrade.DefeatedAnyMechs;
      }

      if (GlobalUpgrade == Upgrade.DefeatedAnyMechs && !(NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)) {
        GlobalUpgrade = Upgrade.InHardMode;
      }

      if (GlobalUpgrade == Upgrade.InHardMode && !Main.hardMode) {
        GlobalUpgrade = Upgrade.DefeatedSkeletron;
      }

      if (GlobalUpgrade == Upgrade.DefeatedSkeletron && !NPC.downedBoss3) {
        GlobalUpgrade = Upgrade.DefeatedDarkBoss;
      }

      if (GlobalUpgrade == Upgrade.DefeatedDarkBoss && !NPC.downedBoss2) {
        GlobalUpgrade = Upgrade.DefeatedEyeOfCthuhlu;
      }

      if (GlobalUpgrade == Upgrade.DefeatedEyeOfCthuhlu && !NPC.downedBoss1) {
        GlobalUpgrade = Upgrade.None;
      }

      if (GlobalUpgrade != oldUpgrade) {
        // string text = "Due to the world being modified, Sein's upgrade regressed from " + oldUpgrade + " to " + GlobalSeinUpgrade;
        // Main.NewText(text);
        // ErrorLogger.Log(text);
      }
    }

    public override void NetSend(BinaryWriter writer) {
      writer.Write((byte)GlobalUpgrade);
    }

    public override void NetReceive(BinaryReader reader) {
      GlobalUpgrade = (Upgrade)reader.ReadByte();
    }
  }
}
