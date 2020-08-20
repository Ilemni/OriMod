using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod {
  public interface IBashable {
    /// <summary>
    /// The player that is bashing this
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
