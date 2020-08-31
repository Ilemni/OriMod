namespace OriMod {
  /// <summary>
  /// Class for leveling where the Level member can be modified.
  /// </summary>
  /// <remarks>This interface should not be used on classes where the level property is intended to be get-only, i.e. dependent on other levels.</remarks>
  public interface ILevelable {
    /// <summary>
    /// The level.
    /// </summary>
    byte Level { get; set; }

    /// <summary>
    /// The max level this can be.
    /// </summary>
    byte MaxLevel { get; }
  }
}
