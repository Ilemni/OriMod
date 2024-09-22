using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

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

  /// <summary>
  /// Checks that the <see cref="BashPlayer"/> is still active, alive, is bashing, and that the entity it is bashing is this.
  /// </summary>
  /// <param name="bashPlayer"></param>
  /// <param name="entity"></param>
  /// <returns></returns>
  public static bool ValidateBash(OriPlayer? bashPlayer, Entity entity) {
    return bashPlayer is { Player: { active: true, dead: false }, ActiveState: Bash bash } &&
      bash.IsBashing(entity);
  }
}
