using System;

namespace OriMod.Utilities;

public static class ArrayExtensions {
  /// <summary>
  /// Assigns multiple indexes of an array to <paramref name="value"/>.
  /// </summary>
  /// <param name="arr">The array to assign values to.</param>
  /// <param name="value">The value to assign to.</param>
  /// <param name="keys">Indices of the array to assign to.</param>
  internal static void AssignValueToKeys<T>(this T[] arr, T value, ReadOnlySpan<int> keys) {
    for (int i = 0, len = keys.Length; i < len; i++) {
      arr[keys[i]] = value;
    }
  }

  /// <summary>
  /// Assigns multiple indexes of an array to <paramref name="value"/>.
  /// </summary>
  /// <param name="arr">The array to assign values to.</param>
  /// <param name="value">The value to assign to.</param>
  /// <param name="keys">Indices of the array to assign to, as ushort values.</param>
  internal static void AssignValueToKeys<T>(this T[] arr, T value, ReadOnlySpan<ushort> keys) {
    for (int i = 0, len = keys.Length; i < len; i++) {
      arr[keys[i]] = value;
    }
  }

  /// <summary>
  /// Assigns multiple indexes of an array to <paramref name="value"/>.
  /// </summary>
  /// <param name="arr">The array to assign values to.</param>
  /// <param name="value">The value to assign to.</param>
  /// <param name="keys">Indices of the array to assign to, as short values.</param>
  internal static void AssignValueToKeys<T>(this T[] arr, T value, ReadOnlySpan<short> keys) {
    for (int i = 0, len = keys.Length; i < len; i++) {
      arr[keys[i]] = value;
    }
  }
}
