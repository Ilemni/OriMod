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
    };
    }

    public override void Load(TagCompound tag) {
      GlobalUpgrade = (Upgrade)tag.GetAsInt("SeinUpgrade");
      ValidateSeinUpgrade();
    }

    internal static void ValidateSeinUpgrade() {
      Upgrade oldUpgrade = GlobalUpgrade;
      GlobalUpgrade = Upgrade.DefeatedMoonLord;

      if (!NPC.downedMoonlord) {
        GlobalUpgrade = Upgrade.DefeatedAllPillars;
        if (!NPC.downedTowers) {
        GlobalUpgrade = Upgrade.DefeatedTwoPillars;
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
            if (!NPC.downedAncientCultist) {
              GlobalUpgrade = Upgrade.DefeatedGolem;
              if (!NPC.downedGolemBoss) {
                GlobalUpgrade = Upgrade.DefeatedPlantera;
                if (!NPC.downedPlantBoss) {
                  GlobalUpgrade = Upgrade.DefeatedAllMechs;
                  if (!(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)) {
                    GlobalUpgrade = Upgrade.DefeatedAnyMechs;
                    if (!(NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)) {
                      GlobalUpgrade = Upgrade.InHardMode;
                      if (!Main.hardMode) {
                        GlobalUpgrade = Upgrade.DefeatedSkeletron;
                        if (!NPC.downedBoss3) {
                          GlobalUpgrade = Upgrade.DefeatedDarkBoss;
                          if (!NPC.downedBoss2) {
                            GlobalUpgrade = Upgrade.DefeatedEyeOfCthuhlu;
                            if (!NPC.downedBoss1) {
                              GlobalUpgrade = Upgrade.None;
                            }
        }
      }
      }
      }
      }
      }
      }
      }
      }
      }
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
