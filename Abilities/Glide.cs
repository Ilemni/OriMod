using OriMod.Utilities;
using System;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for reducing fall velocity to a glide.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing gliding with wings.
  /// </remarks>
  public sealed class Glide : Ability, ILevelable {
    internal Glide(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Glide;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 1;

    internal override bool CanUse =>
      base.CanUse && !Ending && player.velocity.Y * Math.Sign(player.gravDir) > 0 && !player.mount.Active &&
      !abilities.airJump && !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.chargeJump &&
      !abilities.climb && !abilities.dash && !abilities.launch && !abilities.stomp && !abilities.wallChargeJump &&
      !abilities.wallJump;

    private static float MaxFallSpeed => 2f;
    private static float RunSlowdown => 0.125f;
    private static float RunAcceleration => 0.2f;
    private static int StartDuration => 8;
    private static int EndDuration => 10;

    private readonly RandomChar randStart = new RandomChar();
    private readonly RandomChar randActive = new RandomChar();
    private readonly RandomChar randEnd = new RandomChar();

    private bool oldLeft = false;
    private bool oldRight = false;

    protected override void UpdateStarting() {
      if (CurrentTime == 0) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideStart" + randStart.NextNoRepeat(3), 0.8f);
      }
    }

    protected override void UpdateActive() {
      if (player.controlLeft != oldLeft || player.controlRight != oldRight) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + randActive.NextNoRepeat(5), 0.45f);
      }
      oldLeft = player.controlLeft;
      oldRight = player.controlRight;
    }

    protected override void UpdateEnding() {
      if (CurrentTime == 0) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideEnd" + randEnd.NextNoRepeat(3), 0.8f);
      }
    }

    protected override void UpdateUsing() {
      player.maxFallSpeed = MaxFallSpeed;
      if (!oPlayer.UnrestrictedMovement) {
        player.runSlowdown = RunSlowdown;
        player.runAcceleration = RunAcceleration;
      }
    }

    internal override void Tick() {
      if (!InUse && CanUse && !oPlayer.OnWall && input.glide.Current) {
        SetState(State.Starting);
        return;
      }
      if (abilities.dash || abilities.airJump || abilities.burrow) {
        SetState(State.Inactive);
        return;
      }
      if (InUse) {
        if (Starting) {
          if (CurrentTime > StartDuration) {
            SetState(State.Active);
          }
        }
        else if (Ending) {
          if (CurrentTime > EndDuration) {
            SetState(State.Inactive);
          }
        }
        else if (player.velocity.Y * player.gravDir < 0 || oPlayer.OnWall || oPlayer.IsGrounded || !input.glide.Current) {
          SetState(State.Ending);
        }
      }
    }
  }
}
