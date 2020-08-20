using Microsoft.Xna.Framework;

namespace OriMod {
  /// <summary>
  /// For <see cref="Terraria.ModLoader.GlobalNPC"/>s or <see cref="Terraria.ModLoader.GlobalProjectile"/>s that can be Bashed.
  /// </summary>
  public interface IBashable {
    /// <summary>
    /// The player that is bashing this or last bashed this
    /// </summary>
    OriPlayer BashPlayer { get; set; }
    
    /// <summary>
    /// The position where the entity was bashed
    /// </summary>
    Vector2 BashPosition { get; set; }
    
    /// <summary>
    /// Whether or not the entity is being bashed
    /// </summary>
    bool IsBashed { get; set; }

    /// <summary>
    /// Time since this was last Bashed, in frames. 0 if <see cref="IsBashed"/> is <c>true</c>, positive value otherwise.
    /// </summary>
    int FramesSinceLastBash { get; }
  }
}
