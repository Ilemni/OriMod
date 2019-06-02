using Microsoft.Xna.Framework;
using Terraria.GameInput;

namespace OriMod.Movements {
  public class WallJump : Ability {
    internal WallJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private const int EndTime = 12;
    private int CurrTime = 0;

    protected override void UpdateActive() {
      player.velocity.Y = WallJumpVelocity.Y;
      OPlayer.PlayNewSound("Ori/WallJump/seinWallJumps" + OriPlayer.RandomChar(5, ref currRand));
    }
    protected override void UpdateEnding() {
      if (OPlayer.OnWall) player.velocity.Y--;
    }
    protected override void UpdateUsing() {
      player.velocity.X = WallJumpVelocity.X * -player.direction;
      OPlayer.UnrestrictedMovement = true;
    }

    internal override void Tick() {
      if (IsState(States.Ending)) {
        CurrTime++;
        if (CurrTime > EndTime || PlayerInput.Triggers.JustPressed.Right || PlayerInput.Triggers.JustPressed.Left || OPlayer.IsGrounded) {
          State = States.Inactive;
          CanUse = false;
        }
      }
      else if (IsState(States.Active)) {
        State = States.Ending;
        CurrTime = 0;
      }
      else {
        CanUse = OPlayer.OnWall && !OPlayer.IsGrounded && !InUse;
        if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
          State = States.Active;
        }
      }
    }
  }
}