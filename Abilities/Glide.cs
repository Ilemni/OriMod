using OriMod.Utilities;
using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for reducing fall velocity to a glide.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing gliding with wings.
  /// </remarks>
  public sealed class Glide : Ability {
    internal Glide(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Glide;

    internal override bool CanUse =>
      base.CanUse && !Ending &&
      !abilities.airJump.InUse && !abilities.stomp.InUse && !abilities.dash.InUse && !abilities.chargeDash.InUse &&
      !abilities.wallChargeJump.InUse && !abilities.burrow.InUse &&
      player.velocity.Y * Math.Sign(player.gravDir) > 0 && !player.mount.Active;

    private static float MaxFallSpeed => 2f;
    private static float RunSlowdown => 0.125f;
    private static float RunAcceleration => 0.2f;
    private static int StartDuration => 8;
    private static int EndDuration => 10;

    private readonly RandomChar randStart = new RandomChar();
    private readonly RandomChar randActive = new RandomChar();
    private readonly RandomChar randEnd = new RandomChar();

    protected override void UpdateStarting() {
      if (CurrentTime == 0) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideStart" + randStart.NextNoRepeat(3), 0.8f);
      }
    }

    protected override void UpdateActive() {
      if (player.controlLeft || player.controlRight) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + randActive.NextNoRepeat(5), 0.45f);
      }
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
      if (!InUse && CanUse && !oPlayer.OnWall && IsLocal && (OriMod.FeatherKey.JustPressed || OriMod.FeatherKey.Current)) {
        SetState(State.Starting);
        return;
      }
      if (abilities.dash.InUse || abilities.airJump.InUse) {
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
        if (player.velocity.Y * player.gravDir < 0 || oPlayer.OnWall || oPlayer.IsGrounded) {
          SetState(InUse ? State.Ending : State.Inactive);
        }
        else if (IsLocal && OriMod.FeatherKey.JustReleased) {
          SetState(State.Ending);
        }
      }
    }
  }
}
