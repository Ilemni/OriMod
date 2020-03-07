namespace OriMod.Upgrades {
  /// <summary>
  /// A type of Upgrade that Unlocks (or relocks) an IUnlockable.
  /// </summary>
  public class UnlockUpgrade : ActionUpgrade {
    public UnlockUpgrade(string name, int cost, IUnlockable unlockable, Upgrade[] requiredUpgrades, ConditionResult condition = null)
      : base($"{name}.Unlock", cost, requiredUpgrades, condition) {
      this.unlockable = unlockable;
    }

    public readonly IUnlockable unlockable;

    public override void OnEnable() => unlockable.Unlocked = true;
    public override void OnDisable() => unlockable.Unlocked = false;
  }
}
