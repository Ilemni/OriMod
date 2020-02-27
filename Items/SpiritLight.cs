using OriMod.Upgrades;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Items {
  public class SpiritLight : ModItem {
    public override bool Autoload(ref string name) => false; 
    public int Currency { get; }

    public void AddCurrencyToPlayer(Player player) => player.GetModPlayer<OriPlayer>().SpiritLight += Currency;

    #region Handling inventory scenarios
    public override bool CanPickup(Player player) => true;

    public override bool OnPickup(Player player) {
      AddCurrencyToPlayer(player);
      return false;
    }

    // Just in case this were to be modded into the inventory :/
    public override void OnConsumeItem(Player player) => AddCurrencyToPlayer(player);
    #endregion

    #region Item in the world
    public override void Update(ref float gravity, ref float maxFallSpeed) {
      if (gravity > 0.3f) {
        gravity = 0.3f;
      }
      if (maxFallSpeed > 0.5f) {
        maxFallSpeed = 0.5f;
      }
    }

    public override void GrabRange(Player player, ref int grabRange) {
      grabRange = 1;
      
      var oPlayer = player.GetModPlayer<OriPlayer>();
      var upgrades = oPlayer.Upgrades.Upgrades;

      if (upgrades.TryGetValue("SpiritLight-GrabRange-I", out Upgrade upgrade) && upgrade) {
        grabRange += ((SpiritLightGrabRangeUpgrade)upgrade).GrabRange;
      }
      if (upgrades.TryGetValue("SpiritLight-GrabRange-II", out upgrade) && upgrade) {
        grabRange += ((SpiritLightGrabRangeUpgrade)upgrade).GrabRange;
      }
      if (upgrades.TryGetValue("SpiritLight-GrabRange-III", out upgrade) && upgrade) {
        grabRange += ((SpiritLightGrabRangeUpgrade)upgrade).GrabRange;
      }
      if (upgrades.TryGetValue("SpiritLight-GrabRange-IV", out upgrade) && upgrade) {
        grabRange += ((SpiritLightGrabRangeUpgrade)upgrade).GrabRange;
      }
    }

    public override bool ItemSpace(Player player) => true;
    #endregion
  }
}
