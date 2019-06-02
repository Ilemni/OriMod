using Terraria.GameInput;

namespace OriMod.Movements {
  public class AirJump : Ability {
    internal AirJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    
    private const float JumpVelocity = -8.8f;
    private const int EndDuration = 32;
    private const int MaxJumps = 2;
    private int CurrCount = 0;
    private int CurrTime = 0;

    protected override void UpdateActive() {
      if (CurrCount == MaxJumps) {
        OPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + OriPlayer.RandomChar(5), 0.7f);
      }
      else {
        OPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + OriPlayer.RandomChar(5), 0.75f);
      }
      if (player.velocity.Y > JumpVelocity) player.velocity.Y = JumpVelocity;
    }

    internal override void Tick() {
      if (IsState(States.Ending)) {
        CurrTime++;
        if (CurrTime > EndDuration) {
          State = States.Inactive;
          if (CurrCount >= MaxJumps) CanUse = false;
          CurrTime = 0;
        }
      }
      if (IsState(States.Active)) {
        State = States.Ending;
      }
      if (CurrCount <= MaxJumps && !OPlayer.isGrounded) {
        CanUse = true;
      }
      if (OPlayer.isGrounded || /*Handler.bash.inUse ||*/ OPlayer.bashActive || OPlayer.onWall) {
        CurrCount = 0;
        State = States.Inactive;
        CanUse = false;
      }
      if ((IsState(States.Ending) || CanUse) && CurrCount < MaxJumps && PlayerInput.Triggers.JustPressed.Jump) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Cart)) {
          State = States.Active;
          if (Handler.dash.InUse) {
            Handler.dash.CanUse = false;
            Handler.dash.State = States.Inactive;
          }
          CurrTime = 0;
          CurrCount++;
          CanUse = false;
        }
      }
    }
  }
}