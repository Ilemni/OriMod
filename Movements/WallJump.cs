using Microsoft.Xna.Framework;
using Terraria.GameInput;

namespace OriMod.Movements {
  public class WallJump : Ability {
    internal WallJump(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private const int EndTime = 12;
    private int CurrTime = 0;

    protected override void UpdateActive() {
      player.velocity.Y = WallJumpVelocity.Y;
      OPlayer.PlayNewSound("Ori/WallJump/seinWallJumps" + OriPlayer.RandomChar(5, ref currRand));
    }
    protected override void UpdateEnding() {
      if (OPlayer.onWall) player.velocity.Y--;
    }
    protected override void UpdateUsing() {
      player.velocity.X = WallJumpVelocity.X * -player.direction;
      OPlayer.unrestrictedMovement = true;
    }

    internal override void Tick() {
      if (IsState(States.Ending)) {
        CurrTime++;
        if (CurrTime > EndTime || PlayerInput.Triggers.JustPressed.Right || PlayerInput.Triggers.JustPressed.Left || OPlayer.isGrounded) {
          State = States.Inactive;
          CanUse = false;
        }
      }
      else if (IsState(States.Active)) {
        State = States.Ending;
        CurrTime = 0;
      }
      else {
        CanUse = OPlayer.onWall && !OPlayer.isGrounded && !InUse;
        if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
          State = States.Active;
        }
      }
    }
  }
}