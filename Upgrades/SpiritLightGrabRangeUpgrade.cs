namespace OriMod.Upgrades {
  public class SpiritLightGrabRangeUpgrade : Upgrade {
    public SpiritLightGrabRangeUpgrade(string name, int cost, int grabRange, Upgrade[] requiredUpgrades, ConditionResult worldRequirement = null) : base(name, cost, requiredUpgrades, worldRequirement) {
      GrabRange = grabRange;
    }

    public readonly int GrabRange;
  }
}
