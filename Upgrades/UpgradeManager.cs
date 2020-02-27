using System.Collections.Generic;
using Terraria;

namespace OriMod.Upgrades {
  public sealed class UpgradeManager {
    internal UpgradeManager(OriPlayer oPlayer) {
      if (oPlayer.player.whoAmI == Main.myPlayer) {
        Local = this;
      }

      var ab = oPlayer.Abilities;
      Upgrades = new Dictionary<string, Upgrade> {
        ["Sein"] = new UnlockUpgrade("Sein", 100, null, null), // TODO: Sein inherit from IUnlockable
        ["WallJump"] = new UnlockUpgrade("WallJump", 150, ab.wJump, null, UpgradeConditions.DownedBoss1),
        ["AirJump"] = new UnlockUpgrade("AirJump", 250, ab.airJump, null),
        ["Bash"] = new UnlockUpgrade("Bash", 500, ab.bash, null),
        ["Stomp"] = new UnlockUpgrade("Stomp", 800, ab.stomp, null),
        ["Glide"] = new UnlockUpgrade("Glide", 400, ab.glide, null),
        ["Climb"] = new UnlockUpgrade("Climb", 600, ab.climb, null),
        ["ChargeJump"] = new UnlockUpgrade("ChargeJump", 1250, ab.cJump, null),
      };
      AddUpgrade(new UnlockUpgrade("WallChargeJump", 1500, ab.wCJump, new[] { Upgrades["WallJump"], Upgrades["ChargeJump"] }));
      AddUpgrade(new SpiritLightGrabRangeUpgrade("SpiritLight-GrabRange-I", 50, 2, null, null))
        .ChainUpgrade(this, new SpiritLightGrabRangeUpgrade("SpiritLight-GrabRange-II", 300, 3, null, null))
        .ChainUpgrade(this, new SpiritLightGrabRangeUpgrade("SpiritLight-GrabRange-III", 700, 6, null, null))
        .ChainUpgrade(this, new SpiritLightGrabRangeUpgrade("SpiritLight-GrabRange-IV", 1600, 13, null, null));
    }

    public static UpgradeManager Local { get; private set; }

    public readonly Dictionary<string, Upgrade> Upgrades;

    internal Upgrade AddUpgrade(Upgrade u) {
      Upgrades.Add(u.Name, u);
      return u;
    }

    internal static void Unload() => Local = null;
  }

  internal static class UpgradeConditions {
    internal static bool DownedBoss1(out string failReason) {
      if (NPC.downedBoss1) {
        failReason = default;
        return true;
      }
      failReason = "Defeat Eye of Cthuhlu";
      return false;
    }
  }
}
