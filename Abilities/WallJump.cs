using Microsoft.Xna.Framework;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class WallJump : Ability {
    internal WallJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool DoUpdate => (oPlayer.Input(OriPlayer.JustPressed.Jump) && oPlayer.OnWall && !oPlayer.IsGrounded);
    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !InUse && !player.mount.Active && !Handler.wCJump.Charged;

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private int EndTime => 12;
    
    private int CurrTime;
    private int WallDir;
    private int GravDir;

    protected override void UpdateActive() {
      player.velocity.Y = WallJumpVelocity.Y * GravDir;
      oPlayer.PlayNewSound("Ori/WallJump/seinWallJumps" + OriPlayer.RandomChar(5, ref currRand));
    }
    protected override void UpdateEnding() {
      if (oPlayer.OnWall) player.velocity.Y -= GravDir;
    }
    protected override void UpdateUsing() {
      player.velocity.X = WallJumpVelocity.X * -WallDir;
      player.direction = WallDir;
      oPlayer.UnrestrictedMovement = true;
    }

    internal override void Tick() {
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        Active = true;
        WallDir = player.direction;
        GravDir = (int)player.gravDir;
        Handler.climb.Inactive = true;
      }
      else if (Active) {
        Ending = true;
        CurrTime = 0;
      }
      else if (Ending) {
        CurrTime++;
        if (CurrTime > EndTime || PlayerInput.Triggers.JustPressed.Right || PlayerInput.Triggers.JustPressed.Left || oPlayer.IsGrounded) {
          Inactive = true;
        }
      }
    }
  }
}