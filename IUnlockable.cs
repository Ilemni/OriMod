namespace OriMod {
  /// <summary>
  /// Allows objects to be "unlockable," i.e. the player cannot use certain in-game items or abilities until unlocked.
  /// </summary>
  public interface IUnlockable {
    /// <summary>
    /// Unlock state. If false, the player should not be able to use this.
    /// </summary>
    bool Unlocked { get; set; }
  }
}
