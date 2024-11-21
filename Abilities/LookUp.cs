using System;
using AnimLib.Animations;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// StateMachine for the player looking upwards. Pairs with the ability <see cref="ChargeJump"/>.
/// </summary>
public sealed class LookUp(Player player) : OriState(player) {
  public override bool CanEnter() => base.CanEnter() && OriPlayer.IsGrounded &&
    Player is { controlLeft: false, controlRight: false } && Math.Abs(Player.velocity.X) < 0.8f;

  private static int StartDuration => 12;
  private static int EndDuration => 8;

  private bool Starting => ActiveTime < StartDuration;

  private bool Ending => _endingTime > 0;

  private int _endingTime;

  protected override void OnPreUpdate() {
    if (!CanEnter()) {
      CancelState();
      return;
    }

    if (!IsLocal) {
      return;
    }

    if (!(Player.controlUp || OriPlayer.Input.Charge.Current)) {
      if (Starting) {
        CancelState();
        return;
      }

      _endingTime++;
      if (_endingTime > EndDuration) {
        CancelState();
      }
    }
    else {
      _endingTime = 0;
    }
  }

  protected override AnimationOptions? GetAnimationOptions() =>
    Starting ? new AnimationOptions("LookUpStart") :
    Ending ? new AnimationOptions("LookUpStart", isReversed: true) :
    new AnimationOptions("LookUp");
}
