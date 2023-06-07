using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace OriMod; 

/// <summary>
/// For <see cref="GlobalNPC"/>s or <see cref="GlobalProjectile"/>s that can be Bashed.
/// </summary>
public interface IBashable {
  /// <summary>
  /// The player that is bashing this or last bashed this.
  /// </summary>
  OriPlayer BashPlayer { get; set; }

  /// <summary>
  /// The position where the entity was bashed.
  /// </summary>
  Vector2 BashPosition { get; set; }

  /// <summary>
  /// Whether or not the entity is being bashed.
  /// </summary>
  bool IsBashed { get; set; }

  /// <summary>
  /// Time since this was last Bashed, in frames. <see langword="0"/> if <see cref="IsBashed"/> is <see langword="true"/>, otherwise a positive value.
  /// </summary>
  int FramesSinceLastBash { get; }
}
