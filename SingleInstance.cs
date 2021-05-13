using System;

namespace OriMod {
  /// <summary>
  /// Class used to hold a single static reference to an instance of <typeparamref name="T"/>.
  /// <para>Classes inheriting from this should use a private constructor.</para>
  /// </summary>
  /// <typeparam name="T">The type to make Singleton.</typeparam>
  public abstract class SingleInstance<T> where T : SingleInstance<T> {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object Lock = new object();

    /// <summary>
    /// The singleton instance of this type.
    /// </summary>
    public static T Instance {
      get {
        if (_instance is null) {
          Initialize();
        }
        return _instance;
      }
    }
    private static T _instance;

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> if it does not already exist.
    /// </summary>
    public static void Initialize() {
      if (!(_instance is null)) return;
      lock (Lock) {
        if (!(_instance is null)) return;
        _instance = (T)Activator.CreateInstance(typeof(T), true);
        OriMod.OnUnload += Unload;
      }
    }

    /// <summary>
    /// Sets the static reference of <see cref="SingleInstance{T}"/> to <see langword="null"/>. Calls <see cref="IDisposable.Dispose"/> first, if applicable.
    /// </summary>
    private static void Unload() {
      // ReSharper disable once SuspiciousTypeConversion.Global
      if (_instance is IDisposable disposable) {
        disposable.Dispose();
      }
      _instance = null;
    }
  }
}
