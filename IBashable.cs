using Microsoft.Xna.Framework;

namespace OriMod;

public interface IBashable {
  /// <summary>
  /// Time in frames which the previously Bashed entity cannot be bashed again.
  /// </summary>
  int ImmuneTime { get; }

  /// <summary>
  /// The player that is bashing this or last bashed this.
  /// </summary>
  OriPlayer? BashPlayer { get; set; }

  /// <summary>
  /// The position where the entity was bashed.
  /// </summary>
  Vector2 BashPosition { get; set; }

  /// <summary>
  /// Whether the entity is being bashed.
  /// </summary>
  bool IsBashed { get; set; }

  /// <summary>
  /// Time since this was last Bashed, in frames. <see langword="0"/> if <see cref="IsBashed"/> is <see langword="true"/>, otherwise a positive value.
  /// </summary>
  int FramesSinceLastBash { get; }


  /// <summary>
  /// Whether the entity can be bashed. False if it is already bashed, or currently immune to bashing.
  /// </summary>
  /// <returns><see langword="true"/> if the entity is capable of being bashed, otherwise <see langword="false"/></returns>
  bool CanBeBashed() => !IsBashed && FramesSinceLastBash >= ImmuneTime;
}
