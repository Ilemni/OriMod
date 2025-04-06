using System;

namespace OriMod.Abilities;

internal interface IStats<out T> where T : IStats<T> {
  /// <summary>
  /// Array of stat values, indexed by level.
  /// </summary>
  /// <remarks>
  /// This returns by ref so that <see cref="Get"/> can resize it if needed.
  /// </remarks>
  protected static abstract T[] Values { get; }

  /// <summary>
  /// Gets the value of <see cref="Values"/> at the specified index.
  /// The index is 1-based, such that `Get(Level)` where Level is 1 gets the value at index 0.
  /// If the index is out of bounds, it will be clamped.
  /// </summary>
  /// <param name="index"></param>
  /// <returns></returns>
  public static ref T Get(int index) {
    index = Math.Clamp(index - 1, 0, T.Values.Length - 1);
    return ref T.Values[index];
  }
}
