namespace OriMod.Upgrades {
  /// <summary>
  /// A type of Upgrade that Unlocks (or relocks) an IUnlockable.
  /// </summary>
  public class UnlockUpgrade : ActionUpgrade {
    public UnlockUpgrade(string name, int cost, IUnlockable unlockable, Upgrade[] requiredUpgrades, ConditionResult condition = null)
      : base($"{name}.Unlock", cost, requiredUpgrades, () => unlockable.Unlocked = true, () => unlockable.Unlocked = false, condition) {
      this.unlockable = unlockable;
    }

    public readonly IUnlockable unlockable;
  }
}
