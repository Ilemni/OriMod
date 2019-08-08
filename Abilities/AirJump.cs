using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class AirJump : Ability {
    internal AirJump(OriAbilities handler) : base(handler) { }
    public override int id => AbilityID.AirJump;
    internal override bool DoUpdate => InUse || oPlayer.Input(OriPlayer.JustPressed.Jump);
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && CurrCount < MaxJumps && !Active && !Handler.bash.InUse && !player.mount.Active && !Handler.wCJump.InUse;
    
    private float JumpVelocity => 8.8f;
    private int EndDuration => AnimationHandler.PlayerAnim.Tracks["AirJump"].Duration;
    private int MaxJumps => Config.AirJumpCount;
    
    internal int CurrCount;
    private int randDoubleJumpSound;
    private int randTripleJumpSound;
    private int GravDir;
    
    protected override void UpdateActive() {
      if (CurrCount == MaxJumps) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + OriPlayer.RandomChar(5, ref randTripleJumpSound), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + OriPlayer.RandomChar(4, ref randDoubleJumpSound), 0.75f);
      }
      float newVel = -JumpVelocity * ((EndDuration - CurrTime) / EndDuration);
      if (player.velocity.Y > newVel) player.velocity.Y = newVel;
      player.velocity.Y *= GravDir;
      Handler.stomp.Inactive = true;
    }

    internal override void Tick() {
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Active)) {
          Active = true;
          if (Handler.dash.Active) {
            Handler.dash.Inactive = true;
            Handler.dash.PutOnCooldown();
          }
          CurrTime = 0;
          CurrCount++;
          GravDir = (int)player.gravDir;
        }
        return;
      }
      if (oPlayer.IsGrounded || Handler.bash.InUse || oPlayer.OnWall) {
        CurrCount = 0;
        Inactive = true;
      }
      if (Active) {
        Ending = true;
      }
      else if (Ending) {
        CurrTime++;
        if (CurrTime < EndDuration - 1 && player.velocity.Y * Math.Sign(player.gravDir) > 0) {
          CurrTime = EndDuration - 1;
        }
        if (CurrTime > EndDuration) {
          Inactive = true;
          CurrTime = 0;
        }
      }
    }
  }
}