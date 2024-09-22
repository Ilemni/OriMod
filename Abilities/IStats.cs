using System;

namespace OriMod.Abilities;

internal interface IStats<T> where T : IStats<T> {
  protected static abstract ref T[] Values { get; }

  /// <summary>
  /// Exists entirely for stats which go past max level.
  /// </summary>
  protected static abstract T CreateFromLevel(int level);

  public static ref T Get(int index) {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ref var values = ref T.Values;
    int oldLen = values.Length;
    if (index < oldLen) {
      return ref values[index];
    }

    Array.Resize(ref values, index);
    for (int i = oldLen; i < index; i++) {
      values[i] = T.CreateFromLevel(i);
    }

    return ref values[index];
  }
}
