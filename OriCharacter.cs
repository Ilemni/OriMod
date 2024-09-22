using AnimLib;
using OriMod.Abilities;

namespace OriMod;

public sealed class OriCharacter(OriPlayer oriPlayer) : AnimCharacter(oriPlayer) {
  /// <inheritdoc cref="MovementStates"/>
  public MovementStates Move { get; private set; } = null!; // OnInitialize()

  public override OriPlayer ModPlayer => (OriPlayer)base.ModPlayer;
  public override ActivationPriority Priority => ActivationPriority.Default;

  protected override void OnInitialize() {
    // TODO: Create class for arm state and register it here.
    Move = AddChild(new MovementStates(Entity));
  }

  protected override void OnExit() {
    ModPlayer.HasTransformedOnce = true;
  }
}
