using AnimLib.Animations;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// StateMachine for the player crouching. This ability is entirely visual, and is always unlocked.
/// </summary>
public sealed class Crouch(Player player) : OriState(player) {
  public override bool CanEnter() => base.CanEnter() && IsGrounded &&
    (OriMod.ConfigClient.softCrouch || !(Player.controlLeft || Player.controlRight));

  private static int StartDuration => 10;
  private static int EndDuration => 4;

  internal bool Starting => ActiveTime < StartDuration;

  internal bool Ending => _endingTime > 0;

  private int _endingTime;

  protected override void OnUpdate() {
    if (OriMod.ConfigClient.softCrouch) {
      return;
    }

    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
    Player.velocity.X = 0;
    if (Player.controlLeft) {
      Player.controlLeft = false;
      Player.ChangeDir(-1);
    }
    else if (Player.controlRight) {
      Player.controlRight = false;
      Player.ChangeDir(1);
    }

    // if (PlayerInput.Triggers.JustPressed.Jump) { // TODO: Backflip
    //   Vector2 pos = player.position;
    //   pos = new Vector2(pos.X + 4, pos.Y + 52);
    //   pos.ToWorldCoordinates();
    //   if (!TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y].type] && !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y + 1].type]) {
    //     backflipping = true;
    //   }
    // }
  }

  protected override void OnPreUpdate() {
    if (!CanEnter()) {
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

  protected override AnimationOptions? GetAnimationOptions() =>
    Starting ? new AnimationOptions("CrouchStart") :
    Ending ? new AnimationOptions("CrouchStart", isReversed: true) :
    new AnimationOptions("Crouch");
}
