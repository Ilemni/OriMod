using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class WallJump : Ability {
    internal WallJump(AbilityManager handler) : base(handler) { }
    public override int Id => AbilityID.WallJump;

    internal override bool DoUpdate => oPlayer.Input(PlayerInput.Triggers.JustPressed.Jump) && oPlayer.OnWall && !oPlayer.IsGrounded;
    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !InUse && !player.mount.Active && !Manager.wCJump.Charged;

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private int EndTime => 12;

    private int WallDir;
    private int GravDir;

    private readonly RandomChar randChar = new RandomChar();

    protected override void UpdateActive() {
      player.velocity.Y = WallJumpVelocity.Y * GravDir;
      oPlayer.PlayNewSound("Ori/WallJump/seinWallJumps" + randChar.NextNoRepeat(5));
    }

    protected override void UpdateEnding() {
      if (oPlayer.OnWall) {
        player.velocity.Y -= GravDir;
      }
    }

    protected override void UpdateUsing() {
      player.velocity.X = WallJumpVelocity.X * -WallDir;
      player.direction = WallDir;
      oPlayer.UnrestrictedMovement = true;
    }

    internal override void Tick() {
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        SetState(State.Inactive);
        WallDir = player.direction;
        GravDir = (int)player.gravDir;
        Manager.climb.SetState(State.Inactive);
      }
      else if (Active) {
        SetState(State.Ending);
        CurrTime = 0;
      }
      else if (Ending) {
        CurrTime++;
        if (CurrTime > EndTime || PlayerInput.Triggers.JustPressed.Right || PlayerInput.Triggers.JustPressed.Left || oPlayer.IsGrounded) {
          SetState(State.Inactive);
        }
      }
    }
  }
}
