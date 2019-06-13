using Terraria;

namespace OriMod {
  internal class UpgradeHandler {
    internal UpgradeHandler(OriPlayer o) {
      oPlayer = o;
      player = o.player;
    }
    internal Player player;
    internal OriPlayer oPlayer;
    internal Upgrade[] Upgrades;

    // Call this whenever an ability is bought, and when opening ability menu
    internal void UpdateUnlocks(int baseID) {
      Upgrade baseUpgrade = Upgrades[baseID];
      baseUpgrade.ThisUnlocks.ForEach(i => Upgrades[i].CheckUnlockState());
    }
  }
}