using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod {
  public interface IBashable {
    /// <summary>
    /// The player that is bashing this
    /// </summary>
    OriPlayer BashPlayer { get; set; }

    /// <summary>
    /// The entity that is being bashed
    /// </summary>
    Entity BashEntity { get; set; }
    
    /// <summary>
    /// The position where the entity was bashed
    /// </summary>
    Vector2 BashPosition { get; set; }
    
    /// <summary>
    /// Whether or not the entity is being bashed
    /// </summary>
    bool IsBashed { get; set; }
  }
}
