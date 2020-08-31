using OriMod.Animations;
using OriMod.Utilities;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping in the air.
  /// </summary>
  public sealed class AirJump : Ability, ILevelable {
    internal AirJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.AirJump;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 3;

    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && currentCount < MaxJumps && !Active && !abilities.bash.InUse && !player.mount.Active && !abilities.wallChargeJump.InUse;

    private static float JumpVelocity => 8.8f;
    private static int EndDuration => AnimationHandler.Instance.PlayerAnim["AirJump"].duration;
    private static int MaxJumps => Config.AirJumpCount;

    internal ushort currentCount;
    private sbyte gravityDirection;

    private readonly RandomChar rand = new RandomChar();

    protected override void UpdateActive() {
      if (currentCount == MaxJumps) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + rand.NextNoRepeat(5), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + rand.NextNoRepeat(4), 0.75f);
      }
      float newVel = -JumpVelocity * ((EndDuration - CurrentTime) / EndDuration);
      if (player.velocity.Y > newVel) {
        player.velocity.Y = newVel;
      }

      player.velocity.Y *= gravityDirection;
    }

    internal override void Tick() {
      if (CanUse && player.controlJump) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Active)) {
          SetState(State.Active);
          if (abilities.dash.Active) {
            abilities.dash.SetState(State.Inactive);
            abilities.dash.PutOnCooldown();
          }
          currentCount++;
          gravityDirection = (sbyte)player.gravDir;
        }
        return;
      }
      if (oPlayer.IsGrounded || abilities.bash.InUse || oPlayer.OnWall) {
        currentCount = 0;
        SetState(State.Inactive);
      }
      else if (Active) {
        abilities.stomp.SetState(State.Inactive);
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
