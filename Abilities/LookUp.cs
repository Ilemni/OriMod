using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public sealed class LookUp : Ability {
    internal LookUp(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.LookUp;

    internal override bool UpdateCondition => InUse || oPlayer.Input(PlayerInput.Triggers.Current.Up);
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !Manager.crouch.InUse && !Manager.dash.InUse && !Manager.chargeDash.InUse;

    private int StartDuration => 12;
    private int EndDuration => 8;

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && (PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current)) {
          SetState(State.Starting);
          CurrentTime = 0;
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
        CurrentTime = 0;
      }
      else if (!(PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current) && !Ending) {
        if (Active) {
          SetState(State.Ending);
        }
        else {
          SetState(State.Inactive);
        }
        CurrentTime = 0;
        return;
      }
      else if (Starting) {
        CurrentTime++;
        if (CurrentTime > StartDuration) {
          SetState(State.Active);
          CurrentTime = 0;
        }
      }
      else if (Ending) {
        CurrentTime++;
        if (CurrentTime > EndDuration) {
          SetState(State.Inactive);
          CurrentTime = 0;
        }
      }
    }
  }
}
