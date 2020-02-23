using System;

namespace OriMod.Upgrades {
  public class ActionUpgrade : Upgrade {
    public ActionUpgrade(string name, int cost, Upgrade[] requiredUpgrades, Action onEnable, Action onDisable, ConditionResult worldRequirement = null) : base(name, cost, requiredUpgrades, worldRequirement) {
      OnEnable = onEnable;
      OnDisable = onDisable;
    }

    public readonly Action OnEnable;
    public readonly Action OnDisable;
  }
}
