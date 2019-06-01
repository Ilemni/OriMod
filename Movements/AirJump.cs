using Terraria.GameInput;

namespace OriMod.Movements {
  public class AirJump : Movement {
    public AirJump(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }
    
    private const float airJumpVelocity = -8.8f;
    private const int airJumpEndDur = 32;
    private const int airJumpsMax = 2;
    private int airJumpCurrCount = 0;
    public int airJumpCurrTime { get; private set; }

    public override void Active() {
      if (airJumpCurrCount == airJumpsMax) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + OriPlayer.RandomChar(5), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + OriPlayer.RandomChar(5), 0.75f);
      }
      if (player.velocity.Y > airJumpVelocity) player.velocity.Y = airJumpVelocity;
    }

    public override void Tick() {
      if (unlocked) {
        if (IsState(State.Ending)) {
          airJumpCurrTime++;
          if (airJumpCurrTime > airJumpEndDur) {
            state = State.Inactive;
            if (airJumpCurrCount >= airJumpsMax) canUse = false;
            airJumpCurrTime = 0;
          }
        }
        if (IsState(State.Active)) {
          state = State.Ending;
        }
        if (airJumpCurrCount <= airJumpsMax && !oPlayer.isGrounded) {
          canUse = true;
        }
        if (oPlayer.isGrounded || /*Handler.bash.inUse ||*/ oPlayer.bashActive || oPlayer.onWall) {
          airJumpCurrCount = 0;
          state = State.Inactive;
          canUse = false;
        }
        if ((IsState(State.Ending) || canUse) && airJumpCurrCount < airJumpsMax && PlayerInput.Triggers.JustPressed.Jump) {
          if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Cart)) {
            state = State.Active;
            if (Handler.dash.inUse) {
              Handler.dash.canUse = false;
              Handler.dash.state = State.Inactive;
            }
            airJumpCurrTime = 0;
            airJumpCurrCount++;
            canUse = false;
          }
        }
      }
    }
  }
}