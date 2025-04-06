using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.States;
using Microsoft.Xna.Framework;

namespace OriMod.Abilities;

/// <summary>
/// StateMachine for the player crouching. This ability is entirely visual, and is always unlocked.
/// </summary>
public sealed class Crouch : OriState {
  public override bool CanEnter() => base.CanEnter() && Character.IsGrounded &&
    (!OriMod.ConfigClient.softCrouch || !(Player.controlLeft || Player.controlRight));

  private static int StartDuration => 10;
  private static int EndDuration => 4;

  private bool Starting => ActiveSelf && ActiveTime < StartDuration;

  private bool Ending => ActiveSelf && _endingTime > 0;

  private int _endingTime;

  protected override void OnExit(State? toState) {
    _endingTime = 0;
  }

  public override void SetControls() {
    if (Player.controlLeft) {
      Player.controlLeft = false;
      Player.ChangeDir(-1);
    }
    else if (Player.controlRight) {
      Player.controlRight = false;
      Player.ChangeDir(1);
    }
  }

  public override void PostUpdateRunSpeeds() {
    if (OriMod.ConfigClient.softCrouch) {
      return;
    }

    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
    Player.velocity.X = 0;

    // if (PlayerInput.Triggers.JustPressed.Jump) { // TODO: Backflip
    //   Vector2 pos = player.position;
    //   pos = new Vector2(pos.X + 4, pos.Y + 52);
    //   pos.ToWorldCoordinates();
    //   if (!TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y].type] && !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y + 1].type]) {
    //     backflipping = true;
    //   }
    // }
  }

  public override void PostUpdateMiscEffects() {
    if (OriMod.ConfigClient.softCrouch && !CanEnter()) {
      CancelState();
      return;
    }

    if (!Player.controlDown) {
      if (Starting) {
        // Has not fully entered crouch. cancel crouch immediately
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

  public override AnimationOptions? GetAnimationOptions() {
    if (Ending) {
      return Anim.HasTag("CrouchEnd")
        ? new AnimationOptions("CrouchEnd")
        : new AnimationOptions("CrouchStart") { IsReversed = true };
    }

    return Starting && !Player.controlDownHold
      ? new AnimationOptions("CrouchStart")
      : new AnimationOptions("Crouch");
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendBoolean(Starting, color: Color.White);
    ui.DrawAppendBoolean(Ending, color: Color.White);
    ui.DrawAppendBoolean(OriMod.ConfigClient.softCrouch, color: Color.White, key: "Config: Soft Crouch");
  }
}
