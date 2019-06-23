using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Glide : Ability {
    internal Glide(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool DoUpdate => oPlayer.Input(OriMod.FeatherKey.Current || OriMod.FeatherKey.JustReleased);
    private const float MaxFallSpeed = 2f;
    private const float RunSlowdown = 0.125f;
    private const float RunAcceleration = 0.2f;
    private const int StartDuration = 8;
    private const int EndDuration = 10;
    private int CurrTime = 0;

    internal override bool CanUse =>
      base.CanUse && !Ending &&
      !Handler.dash.InUse && !Handler.cDash.InUse && !Handler.airJump.InUse && !Handler.wCJump.InUse && !Handler.burrow.InUse &&
      player.velocity.Y * Math.Sign(player.gravDir) > 0 && !player.mount.Active && !Handler.burrow.AutoBurrow;

    protected override void UpdateUsing() {
      if (PlayerInput.Triggers.JustPressed.Left || PlayerInput.Triggers.JustPressed.Right) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + OriPlayer.RandomChar(5), 0.45f);
      }
      player.maxFallSpeed = MaxFallSpeed;
      if (!oPlayer.UnrestrictedMovement) {
        player.runSlowdown = RunSlowdown;
        player.runAcceleration = RunAcceleration;
      }
    }
    protected override void UpdateStarting() {
      if (CurrTime == 0) oPlayer.PlayNewSound("Ori/Glide/seinGlideStart" + OriPlayer.RandomChar(3), 0.8f);
    }
    protected override void UpdateEnding() {
      if (CurrTime == 0) oPlayer.PlayNewSound("Ori/Glide/seinGlideEnd" + OriPlayer.RandomChar(3), 0.8f);
    }
    internal override void Tick() {
      if (!InUse && CanUse && !oPlayer.OnWall && (OriMod.FeatherKey.JustPressed || OriMod.FeatherKey.Current)) {
        Starting = true;
        CurrTime = 0;
        return;
      }
      if (Handler.dash.InUse || Handler.airJump.InUse) {
        Inactive = true;
        CurrTime = 0;
        return;
      }
      if (InUse) {
        if (Starting) {
          CurrTime++;
          if (CurrTime > StartDuration) {
            Active = true;
            CurrTime = 0;
          }
        }
        else if (Ending) {
          CurrTime++;
          if (CurrTime > EndDuration) {
            Inactive = true;
          }
        }
        if (player.velocity.Y * player.gravDir < 0 || oPlayer.OnWall || oPlayer.IsGrounded) {
          if (InUse) {
            Ending = true;
          }
          else {
            Inactive = true;
          }
        } 
        else if (OriMod.FeatherKey.JustReleased) {
          Ending = true;
          CurrTime = 0;
        }
      }
    }
  }
}