using System;

namespace OriMod {
  /// <summary>
  /// Class used to hold a static reference to an instance of T. Classes inheriting from this should use a private constructor.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class SingleInstance<T> where T : SingleInstance<T> {
    private static readonly object _lock = new object();
    
    /// <summary>
    /// The instance of this type
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

    public static void Initialize() {
      if (_instance is null) {
        lock (_lock) {
          if (_instance is null) {
            _instance = (T)Activator.CreateInstance(typeof(T), true);
          }
        }
      }
    }

    /// <summary>
    /// Sets the static reference to null. If this type is IDisposable, Dispose is called first.
    /// </summary>
    public static void Unload() {
      if (_instance is IDisposable dispoable) {
        dispoable.Dispose();
      }
      _instance = null;
    }
  }
}
