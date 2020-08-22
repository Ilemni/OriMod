using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace OriMod {
  public class OriWorld : ModWorld {
    /// <summary>
    /// Enum representing world progression for Ori-based upgrades.
    /// </summary>
    public enum Upgrade : byte {
      None = 0,
      /// <summary>
      /// Defeated Eye of Cthuhlu, tied to <see cref="NPC.downedBoss1"/>.
      /// </summary>
      DefeatedEyeOfCthuhlu = 1,
      /// <summary>
      /// Defeated either Brain of Cthuhlu or Eater of Worlds, tied to <see cref="NPC.downedBoss2"/>.
      /// </summary>
      DefeatedDarkBoss = 2,
      /// <summary>
      /// Defeated Skeletron, tied to <see cref="NPC.downedBoss3"/>.
      /// </summary>
      DefeatedSkeletron = 3,
      /// <summary>
      /// Entered Hard Mode, tied to <see cref="Main.hardMode"/>.
      /// </summary>
      InHardMode = 4,
      /// <summary>
      /// Defeated at least one Mechanical Boss.
      /// </summary>
      DefeatedAnyMechs = 5,
      /// <summary>
      /// Defeated all Mechanical Bosses.
      /// </summary>
      DefeatedAllMechs = 6,
      /// <summary>
      /// Defeated Plantera, tied to <see cref="NPC.downedPlantBoss"/>.
      /// </summary>
      DefeatedPlantera = 7,
      /// <summary>
      /// Defeated Golem, tied to <see cref="NPC.downedGolemBoss"/>.
      /// </summary>
      DefeatedGolem = 8,
      /// <summary>
      /// Defeated Lunar Cultist, tied to <see cref="NPC.downedAncientCultist"/>.
      /// </summary>
      DefeatedLunarCultist = 9,
      /// <summary>
      /// Defeated at least two Celestial Pillars.
      /// </summary>
      DefeatedTwoPillars = 10,
      /// <summary>
      /// Defeated all four Celestial Pillars, tied to <see cref="NPC.downedTowers"/>.
      /// </summary>
      DefeatedAllPillars = 11,
      /// <summary>
      /// Defeated Moon Lord, tied to <see cref="NPC.downedMoonlord"/>.
      /// </summary>
      DefeatedMoonLord = 12
    }

    /// <summary>
    /// Upgrade will determine player abilities.
    /// </summary>
    public static Upgrade GlobalUpgrade { get; private set; }

    public override bool Autoload(ref string name) => true;

    /// <summary>
    /// Update <see cref="GlobalUpgrade"/> based on <paramref name="upgrade"/>. Called when a boss is defeated.
    /// <para><paramref name="upgrade"/> value should be based on bosses represented in <see cref="Upgrade"/>.</para>
    /// </summary>
    /// <param name="upgrade"></param>
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
        ["SeinUpgrade"] = (byte)GlobalUpgrade,
      };
    }

    public override void Load(TagCompound tag) {
      GlobalUpgrade = (Upgrade)tag.GetAsInt("SeinUpgrade");
      ValidateSeinUpgrade();
    }

    /// <summary>
    /// Ensures that <see cref="GlobalUpgrade"/> is correct.
    /// </summary>
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
