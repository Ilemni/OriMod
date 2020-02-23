using OriMod.Utilities;
using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Glide : Ability {
    internal Glide(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Glide;

    internal override bool DoUpdate => oPlayer.Input(OriMod.FeatherKey.Current || OriMod.FeatherKey.JustReleased);
    internal override bool CanUse =>
      base.CanUse && !Ending &&
      !Manager.airJump.InUse && !Manager.stomp.InUse && !Manager.dash.InUse && !Manager.cDash.InUse &&
      !Manager.wCJump.InUse && !Manager.burrow.InUse &&
      player.velocity.Y * Math.Sign(player.gravDir) > 0 && !player.mount.Active;

    private float MaxFallSpeed => 2f;
    private float RunSlowdown => 0.125f;
    private float RunAcceleration => 0.2f;
    private int StartDuration => 8;
    private int EndDuration => 10;

    private readonly RandomChar randCharStart = new RandomChar();
    private readonly RandomChar randCharActive = new RandomChar();
    private readonly RandomChar randCharEnd = new RandomChar();

    protected override void UpdateStarting() {
      if (CurrTime == 0) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideStart" + randCharStart.NextNoRepeat(3), 0.8f);
      }
    }

    protected override void UpdateActive() {
      if (PlayerInput.Triggers.JustPressed.Left || PlayerInput.Triggers.JustPressed.Right) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + randCharActive.NextNoRepeat(5), 0.45f);
      }
    }

    protected override void UpdateEnding() {
      if (CurrTime == 0) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideEnd" + randCharEnd.NextNoRepeat(3), 0.8f);
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
      if (!InUse && CanUse && !oPlayer.OnWall && (OriMod.FeatherKey.JustPressed || OriMod.FeatherKey.Current)) {
        SetState(State.Starting);
        CurrTime = 0;
        return;
      }
      if (Manager.dash.InUse || Manager.airJump.InUse) {
        SetState(State.Inactive);
        CurrTime = 0;
        return;
      }
      if (InUse) {
        if (Starting) {
          CurrTime++;
          if (CurrTime > StartDuration) {
            SetState(State.Active);
            CurrTime = 0;
          }
        }
        else if (Ending) {
          CurrTime++;
          if (CurrTime > EndDuration) {
            SetState(State.Inactive);
          }
        }
        if (player.velocity.Y * player.gravDir < 0 || oPlayer.OnWall || oPlayer.IsGrounded) {
          if (InUse) {
            SetState(State.Ending);
          }
          else {
            SetState(State.Inactive);
          }
        }
        else if (OriMod.FeatherKey.JustReleased) {
          SetState(State.Ending);
          CurrTime = 0;
        }
      }
    }
  }
}
