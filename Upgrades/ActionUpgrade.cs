namespace OriMod.Upgrades {
  public abstract class ActionUpgrade : Upgrade {
    public ActionUpgrade(string name, int cost, Upgrade[] requiredUpgrades, ConditionResult worldRequirement = null) : base(name, cost, requiredUpgrades, worldRequirement) { }

    /// <summary>
    /// Called when the upgrade is enabled.
    /// </summary>
    public abstract void OnEnable();
    /// <summary>
    /// Called when the upgrade is disabled.
    /// </summary>
    public abstract void OnDisable();
  }
}
