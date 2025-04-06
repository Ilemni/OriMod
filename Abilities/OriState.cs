using AnimLib.States;
using OriMod.Animations;

namespace OriMod.Abilities;

public abstract class OriState : State {
  public override OriCharacter Character => (OriCharacter)base.Character!;

  public OriAnimation Anim => Character.Anim;
  protected OriInput Input => Character.Input;

  protected float AnimationSpeed => Player.webbed ? 0.3f : 1;

  /// <summary>
  /// Shorthand for <see cref="State.TriggerState{T}"/>
  /// </summary>
  protected void CancelState() => TriggerState<NoAbility>();
}
