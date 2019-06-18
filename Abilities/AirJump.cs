using Terraria.GameInput;

namespace OriMod.Abilities {
  public class AirJump : Ability {
    internal AirJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && CurrCount < MaxJumps && !Active && !Handler.bash.InUse && !player.mount.Active;
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
      Handler.stomp.Inactive = true;
    }

    internal override void Tick() {
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Active)) {
          Active = true;
          Handler.dash.Inactive = true;
          CurrTime = 0;
          CurrCount++;
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
        if (CurrTime > EndDuration) {
          Inactive = true;
          CurrTime = 0;
        }
      }
    }
  }
}