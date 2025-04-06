using System;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.States;
using Microsoft.Xna.Framework;

namespace OriMod.Abilities;

/// <summary>
/// StateMachine for the player looking upwards. Pairs with the ability <see cref="ChargeJump"/>.
/// </summary>
public sealed class LookUp : OriState {
  public override bool CanEnter() => base.CanEnter() && Character.IsGrounded &&
    Player is { controlLeft: false, controlRight: false } && Math.Abs(Player.velocity.X) < 0.8f;

  private static int StartDuration => 12;
  private static int EndDuration => 8;

  private bool Starting => ActiveTime < StartDuration;

  private bool Ending => _endingTime > 0;

  private int _endingTime;

  [HookCondition(HookConditionFlags.Strict)]
  public override void PostUpdateMiscEffects() {
    if (!CanEnter()) {
      CancelState();
      return;
    }

    if (!IsLocal) {
      return;
    }

    if (Player.controlUp || Input.Charge.Current) {
      _endingTime = 0;
      return;
    }

    if (Starting) {
      CancelState();
      return;
    }

    _endingTime++;
    if (_endingTime > EndDuration) {
      CancelState();
    }
  }

  public override AnimationOptions? GetAnimationOptions() =>
    Starting ? new AnimationOptions("LookUpStart") :
    Ending ? new AnimationOptions("LookUpStart") { IsReversed = true } :
    new AnimationOptions("LookUp");

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Starting", ActiveTime, StartDuration, new Color(83, 191, 102));
    ui.DrawAppendLabelProgressBar("Ending", _endingTime, EndDuration, new Color(191, 83, 83));
  }
}
