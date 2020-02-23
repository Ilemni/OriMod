using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class LookUp : Ability {
    internal LookUp(AbilityManager handler) : base(handler) { }
    public override int Id => AbilityID.LookUp;

    internal override bool DoUpdate => InUse || oPlayer.Input(PlayerInput.Triggers.Current.Up);
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !Manager.crouch.InUse && !Manager.dash.InUse && !Manager.cDash.InUse;

    private int StartDuration => 12;
    private int EndDuration => 8;

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && (PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current)) {
          SetState(State.Starting);
          CurrTime = 0;
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
        CurrTime = 0;
      }
      else if (!(PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current) && !Ending) {
        if (Active) {
          SetState(State.Ending);
        }
        else {
          SetState(State.Inactive);
        }
        CurrTime = 0;
        return;
      }
      else if (Starting) {
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
          CurrTime = 0;
        }
      }
    }
  }
}
