using OriMod.Animations;
using OriMod.Utilities;
using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping in the air.
  /// </summary>
  public sealed class AirJump : Ability {
    internal AirJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.AirJump;

    internal override bool UpdateCondition => PlayerInput.Triggers.JustPressed.Jump;
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && CurrentCount < MaxJumps && !Active && !Manager.bash.InUse && !player.mount.Active && !Manager.wallChargeJump.InUse;

    private float JumpVelocity => 8.8f;
    private int EndDuration => AnimationHandler.Instance.PlayerAnim["AirJump"].duration;
    private int MaxJumps => Config.AirJumpCount;
    
    internal int CurrentCount;
    private int GravityDirection;

    private readonly RandomChar rand = new RandomChar();

    protected override void UpdateActive() {
      if (CurrentCount == MaxJumps) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + rand.NextNoRepeat(5), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + rand.NextNoRepeat(4), 0.75f);
      }
      float newVel = -JumpVelocity * ((EndDuration - CurrentTime) / EndDuration);
      if (player.velocity.Y > newVel) {
        player.velocity.Y = newVel;
      }

      player.velocity.Y *= GravityDirection;
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
          CurrentCount++;
          GravityDirection = (int)player.gravDir;
        }
        return;
      }
      if (oPlayer.IsGrounded || Manager.bash.InUse || oPlayer.OnWall) {
        CurrentCount = 0;
        SetState(State.Inactive);
      }
      else if (Active) {
        SetState(State.Ending);
      }
      else if (Ending) {
        if (CurrentTime > EndDuration || player.velocity.Y * player.gravDir > 0) {
          SetState(State.Inactive);
        }
      }
    }
  }
}
