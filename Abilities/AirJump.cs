using Terraria.GameInput;

namespace OriMod.Abilities {
  public class AirJump : Ability {
    internal AirJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && CurrCount < MaxJumps && State != States.Active && !Handler.bash.InUse && !player.mount.Active;
    private const float JumpVelocity = 8.8f;
    private const int EndDuration = 32;
    private const int MaxJumps = 2;
    private int CurrCount = 0;
    private int CurrTime = 0;
    private int randDoubleJumpSound = 1;
    private int randTripleJumpSound = 1;

    protected override void UpdateActive() {
      if (CurrCount == MaxJumps) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + OriPlayer.RandomChar(5, ref randTripleJumpSound), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + OriPlayer.RandomChar(4, ref randDoubleJumpSound), 0.75f);
      }
      float newVel = -JumpVelocity * ((EndDuration - CurrTime) / EndDuration);
      if (player.velocity.Y > newVel) player.velocity.Y = newVel;
      Handler.stomp.State = States.Inactive;
      // if (CurrTime == 0) player.gravity = -JumpVelocity;
    }

    internal override void Tick() {
      if (IsState(States.Ending)) {
        CurrTime++;
        if (CurrTime > EndDuration) {
          State = States.Inactive;
          CurrTime = 0;
        }
      }
      if (IsState(States.Active)) {
        State = States.Ending;
      }
      if (oPlayer.IsGrounded || Handler.bash.InUse || oPlayer.OnWall) {
        CurrCount = 0;
        State = States.Inactive;
      }
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Active)) {
          State = States.Active;
          if (Handler.dash.InUse) {
            Handler.dash.State = States.Inactive;
          }
          CurrTime = 0;
          CurrCount++;
        }
      }
    }
  }
}