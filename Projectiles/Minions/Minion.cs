using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions; 

/// <summary>
/// Base class for minion projectiles.
/// </summary>
public abstract class Minion : ModProjectile {
  /// <summary>
  /// Write minion AI in <see cref="Behavior"/>.
  /// </summary>
  public sealed override void AI() {
    CheckActive();
    Behavior();
  }

  /// <summary>
  /// Use this to keep the projectile alive depending on conditions.
  /// </summary>
  protected abstract void CheckActive();

  /// <summary>
  /// Behavior of the minion.
  /// </summary>
  protected abstract void Behavior();
}
