using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class LookUp : Ability {
    internal LookUp(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !Handler.crouch.InUse && !Handler.dash.InUse && !Handler.cDash.InUse;
    private const int StartDuration = 12;
    private const int EndDuration = 8;
    private int CurrTime = 0;

    internal override void Tick() {
      if (InUse) {
        if (!CanUse) {
          State = States.Inactive;
          CurrTime = 0;
          return;
        }
        if (!(PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current) && State != States.Ending) {
          State = State == States.Active ? States.Ending : States.Inactive;
          CurrTime = 0;
          return;
        }
        CurrTime++;
        if (State == States.Starting) {
          if (CurrTime > StartDuration) {
            State = States.Active;
            CurrTime = 0;
          }
        }
        else if (State == States.Ending) {
          if (CurrTime > EndDuration) {
            State = States.Inactive;
            CurrTime = 0;
          }
        }
      }
      else {
        if (CanUse && (PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current)) {
          State = States.Starting;
          CurrTime = 0;
        }
      }
    }
  }
}