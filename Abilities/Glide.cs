using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for reducing fall velocity to a glide.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing gliding with wings.
  /// </remarks>
  public sealed class Glide : Ability<OriAbilityManager>, ILevelable {
    public override int Id => AbilityId.Glide;
    public override int Level => (this as ILevelable).Level;
    int ILevelable.Level { get; set; }
    int ILevelable.MaxLevel => 1;
    public override bool Unlocked => Level > 0;

    public override bool CanUse =>
      base.CanUse && !Ending && player.velocity.Y * Math.Sign(player.gravDir) > 0 && !player.mount.Active &&
      !abilities.airJump && !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.chargeJump &&
      !abilities.climb && !abilities.dash && !abilities.launch && !abilities.stomp && !abilities.wallChargeJump &&
      !abilities.wallJump;

    private static float RunSlowdown => 0.125f;
    private static float RunAcceleration => 0.2f;
    private static int StartDuration => 8;
    private static int EndDuration => 10;

    private readonly RandomChar _randStart = new RandomChar();
    private readonly RandomChar _randActive = new RandomChar();
    private readonly RandomChar _randEnd = new RandomChar();

    private bool _oldLeft;
    private bool _oldRight;

    public override void UpdateStarting() {
      if (stateTime == 0) {
        abilities.oPlayer.PlaySound("Ori/Glide/seinGlideStart" + _randStart.NextNoRepeat(3), 0.8f);
      }
    }

    public override void UpdateActive() {
      if (player.controlLeft != _oldLeft || player.controlRight != _oldRight) {
        abilities.oPlayer.PlaySound("Ori/Glide/seinGlideMoveLeftRight" + _randActive.NextNoRepeat(5), 0.45f);
      }
      _oldLeft = player.controlLeft;
      _oldRight = player.controlRight;
    }

    public override void UpdateEnding() {
      if (stateTime == 0) {
        abilities.oPlayer.PlaySound("Ori/Glide/seinGlideEnd" + _randEnd.NextNoRepeat(3), 0.8f);
      }
    }

    public override void UpdateUsing() {
      player.maxFallSpeed = MathHelper.Clamp(player.gravity * 5, 1f, 2f);
      if (abilities.oPlayer.UnrestrictedMovement) return;
      player.runSlowdown = RunSlowdown;
      player.runAcceleration = RunAcceleration;
    }

    public override void PreUpdate() {
      if (!InUse && CanUse && !abilities.oPlayer.OnWall && abilities.oPlayer.input.glide.Current) {
        SetState(AbilityState.Starting);
        return;
      }
      if (abilities.dash || abilities.airJump || abilities.burrow || abilities.launch) {
        SetState(AbilityState.Inactive);
        return;
      }

      if (!InUse) return;
      if (Starting) {
        if (stateTime > StartDuration) {
          SetState(AbilityState.Active);
        }
      }
      else if (Ending) {
        if (stateTime > EndDuration) {
          SetState(AbilityState.Inactive);
        }
      }
      else if (player.velocity.Y * player.gravDir < 0 || abilities.oPlayer.OnWall
        || abilities.oPlayer.IsGrounded || !abilities.oPlayer.input.glide.Current) {
        SetState(AbilityState.Ending);
      }
    }
  }
}
