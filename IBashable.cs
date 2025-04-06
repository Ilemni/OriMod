using OriMod.Abilities;

namespace OriMod;

public interface IBashable {
  /// <summary>
  /// Time in ticks which the previously Bashed entity cannot be bashed again.
  /// </summary>
  int ImmuneTime { get; }

  /// <summary>
  /// The player that is bashing this or last bashed this.
  /// </summary>
  Bash? Bash { get; set; }

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

  public void SetBash(Bash bash) {
    IsBashed = true;
    Bash = bash;
    FramesUntilBashable = ImmuneTime;
  }

  /// <summary>
  /// Clears the Bash Player, only if it is the matching player.
  /// </summary>
  /// <param name="bash"></param>
  public void TryClearBash(Bash bash) {
    if (Bash is null || Bash.Player.whoAmI == bash.Player.whoAmI) {
      ClearBashPlayer();
    }
  }

  public void ClearBashPlayer() {
    IsBashed = false;
    Bash = null;
  }
}
