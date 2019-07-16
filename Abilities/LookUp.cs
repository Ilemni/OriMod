using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class LookUp : Ability {
    internal LookUp(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    public override int id => AbilityID.LookUp;
    internal override bool DoUpdate => InUse || oPlayer.Input(OriPlayer.Current.Up);
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !Handler.crouch.InUse && !Handler.dash.InUse && !Handler.cDash.InUse;
    
    private int StartDuration => 12;
    private int EndDuration => 8;

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && (PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current)) {
          Starting = true;
          CurrTime = 0;
        }
      }
      else if (!CanUse) {
        Inactive = true;
        CurrTime = 0;
      }
      else if (!(PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current) && !Ending) {
        if (Active) {
          Ending = true;
        }
        else {
          Inactive = true;
        }
        CurrTime = 0;
        return;
      }
      else if (Starting) {
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
          CurrTime = 0;
        }
      }
    }
  }
}