using System;

namespace OriMod.Abilities;

internal interface IStats<T> where T : IStats<T> {
  /// <summary>
  /// Array of stat values, indexed by level.
  /// </summary>
  /// <remarks>
  /// This returns by ref so that <see cref="Get"/> can resize it if needed.
  /// </remarks>
  protected static abstract ref T[] Values { get; }

  /// <summary>
  /// Exists entirely for stats which go past max level.
  /// </summary>
  protected static abstract T CreateFromLevel(int level);

  public static int MaxSize => 128;

  /// <summary>
  /// Gets the value of <see cref="Values"/> at the specified index.
  /// If the specified index is larger than the value, the array will be resized to accomodate it.
  /// </summary>
  /// <param name="index"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="index"/> was negative or greater than <see cref="MaxSize"/>
  /// </exception>
  public static ref T Get(int index) {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, MaxSize);

    ref var values = ref T.Values;
    int oldLen = values.Length;
    if (index < oldLen) {
      return ref values[index];
    }

    Array.Resize(ref values, index + 1);
    for (int i = oldLen; i <= index; i++) {
      values[i] = T.CreateFromLevel(i);
    }

    return ref values[index];
  }
}
