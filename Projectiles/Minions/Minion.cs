using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  /// <summary>
  /// Base class for minion projectiles.
  /// </summary>
  public abstract class Minion : ModProjectile {
    /// <summary>
    /// Write minion AI in <see cref="Behavior"/>.
    /// </summary>
    public override sealed void AI() {
      CheckActive();
      Behavior();
    }

    /// <summary>
    /// Use this to keep the projectile alive depending on conditions.
    /// </summary>
    internal abstract void CheckActive();

    /// <summary>
    /// Behavior of the minion.
    /// </summary>
    internal abstract void Behavior();
  }
}
