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
  /// Whether the entity is being bashed.
  /// </summary>
  bool IsBashed { get; set; }

  /// <summary>
  /// Time until this can be bashed.
  /// Equal to <see cref="ImmuneTime"/> if <see cref="IsBashed"/> is <see langword="true"/>.
  /// If this value is not <c>0</c>, the entity cannot be bashed.
  /// </summary>
  int FramesUntilBashable { get; set; }


  /// <summary>
  /// Whether the entity's current state allows it to be bashed.
  /// Returns <see langword="false"/> if it is already bashed, or currently immune to bashing.
  /// </summary>
  /// <returns>
  /// <see langword="true"/> if the entity can currently be bashed, otherwise <see langword="false"/>
  /// </returns>
  public bool CanBeBashed() => !IsBashed && FramesUntilBashable == 0;

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

  public void SetBashPlayer(OriPlayer bashPlayer) {
    IsBashed = true;
    BashPlayer = bashPlayer;
    FramesUntilBashable = ImmuneTime;
  }

  public void ClearBashPlayer() {
    IsBashed = false;
    BashPlayer = null;
  }
}
