using AnimLib.States;
using Terraria;

namespace OriMod.Abilities;

public abstract class OriState(Player player) : State(player) {
  // ReSharper disable once InconsistentNaming

  protected Player Player => (Player)Entity;
  protected OriPlayer OriPlayer { get; private set; } = null!; // OnInitialize()
  protected OriInput Input { get; private set; } = null!; // OnInitialize()

  protected bool OnWall => OriPlayer.OnWall;
  protected bool IsGrounded => OriPlayer.IsGrounded;

  protected override void OnInitialize() {
    OriPlayer = Player.GetModPlayer<OriPlayer>();
    Input = OriPlayer.Input;
  }

  protected void RestoreAirJumps() => OriPlayer.RestoreAirJumps();

  /// <summary>
  /// Shorthand for <see cref="State.TriggerState{T}"/>
  /// </summary>
  protected void CancelState() => TriggerState<NoAbility>();
}
