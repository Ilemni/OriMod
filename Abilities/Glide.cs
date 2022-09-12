//using AnimLib.Abilities;
using System;
using Microsoft.Xna.Framework;
using OriMod.Utilities;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for reducing fall velocity to a glide.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing gliding with wings.
  /// </remarks>
  public sealed class Glide : Ability, ILevelable {
    internal Glide(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityId.Glide;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 1;

    internal override bool CanUse =>
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

    protected override void UpdateStarting() {
      if (CurrentTime == 0) {
        oPlayer.PlaySound("Ori/Glide/seinGlideStart" + _randStart.NextNoRepeat(3), 0.8f);
      }
    }

    protected override void UpdateActive() {
      if (player.controlLeft != _oldLeft || player.controlRight != _oldRight) {
        oPlayer.PlaySound("Ori/Glide/seinGlideMoveLeftRight" + _randActive.NextNoRepeat(5), 0.45f);
      }
      _oldLeft = player.controlLeft;
      _oldRight = player.controlRight;
    }

    protected override void UpdateEnding() {
      if (CurrentTime == 0) {
        oPlayer.PlaySound("Ori/Glide/seinGlideEnd" + _randEnd.NextNoRepeat(3), 0.8f);
      }
    }

    protected override void UpdateUsing() {
      player.maxFallSpeed = MathHelper.Clamp(player.gravity * 5, 1f, 2f);
      if (oPlayer.UnrestrictedMovement) return;
      player.runSlowdown = RunSlowdown;
      player.runAcceleration = RunAcceleration;
    }

    internal override void Tick() {
      if (!InUse && CanUse && !oPlayer.OnWall && input.glide.Current) {
        SetState(State.Starting);
        return;
      }
      if (abilities.dash || abilities.airJump || abilities.burrow || abilities.launch) {
        SetState(State.Inactive);
        return;
      }
      
      if (!InUse) return;
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
