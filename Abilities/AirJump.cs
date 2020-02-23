using OriMod.Animations;
using OriMod.Utilities;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class AirJump : Ability {
    internal AirJump(AbilityManager handler) : base(handler) { }
    public override int Id => AbilityID.AirJump;

    internal override bool DoUpdate => InUse || oPlayer.Input(PlayerInput.Triggers.JustPressed.Jump);
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && CurrCount < MaxJumps && !Active && !Manager.bash.InUse && !player.mount.Active && !Manager.wCJump.InUse;

    private float JumpVelocity => 8.8f;
    private int EndDuration => AnimationHandler.PlayerAnim.Tracks["AirJump"].Duration;
    private int MaxJumps => Config.AirJumpCount;
    
    internal int CurrCount;
    private int GravDir;

    private readonly RandomChar airJumpRand = new RandomChar();

    protected override void UpdateActive() {
      if (CurrCount == MaxJumps) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + airJumpRand.NextNoRepeat(5), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + airJumpRand.NextNoRepeat(4), 0.75f);
      }
      float newVel = -JumpVelocity * ((EndDuration - CurrTime) / EndDuration);
      if (player.velocity.Y > newVel) {
        player.velocity.Y = newVel;
      }

      player.velocity.Y *= GravDir;
      Manager.stomp.SetState(State.Inactive);
    }

    internal override void Tick() {
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Active)) {
          SetState(State.Active);
          if (Manager.dash.Active) {
            Manager.dash.SetState(State.Inactive);
            Manager.dash.PutOnCooldown();
          }
          CurrTime = 0;
          CurrCount++;
          GravDir = (int)player.gravDir;
        }
        return;
      }
      if (oPlayer.IsGrounded || Manager.bash.InUse || oPlayer.OnWall) {
        CurrCount = 0;
        SetState(State.Inactive);
      }
      if (Active) {
        SetState(State.Ending);
      }
      else if (Ending) {
        CurrTime++;
        if (CurrTime > EndDuration || player.velocity.Y * player.gravDir > 0) {
          SetState(State.Inactive);
          CurrTime = 0;
        }
      }
    }
  }
}
