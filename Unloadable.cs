using System;

namespace OriMod;

/// <summary>
/// Class to handle the unloading of static fields. This class should strictly only be used for static fields.
/// </summary>
internal static class Unloadable {
    /// <summary>
    /// Initialize a static field with the given value, and register an unload action to null the static field.
    /// </summary>
    /// <param name="value">Value to initialize the static field to</param>
    /// <param name="unload">Unload action to set the static field to null</param>
    /// <typeparam name="T">Class type</typeparam>
    /// <returns><paramref name="value"/></returns>
    /// <example>
    /// <code>
    /// public static int[] myArray => _myArray ??=
    ///   Unloadable.New(
    ///     new[] {1, 2, 3, 4},
    ///     () => _myArray = null
    ///   );
    /// private static int[] _myArray;
    /// </code>
    /// </example>
    public static T New<T>(T value, Action unload) where T : class {
        OnUnload += unload;
        return value;
    }

    public static void Unload() {
        OnUnload?.Invoke();
        OnUnload = null;
    }

    public static event Action OnUnload;
}