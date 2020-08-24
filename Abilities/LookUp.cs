using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for looking up. Pairs with the ability <see cref="ChargeJump"/>.
  /// <para>This ability on its own is entirely visual, and is always unlocked.</para>
  /// </summary>
  public sealed class LookUp : Ability {
    internal LookUp(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.LookUp;
    public override bool Unlocked => true;

    internal override bool UpdateCondition => InUse || oPlayer.Input(PlayerInput.Triggers.Current.Up);
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !Manager.crouch.InUse && !Manager.dash.InUse && !Manager.chargeDash.InUse;

    private int StartDuration => 12;
    private int EndDuration => 8;

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && (PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current)) {
          SetState(State.Starting);
          currentTime = 0;
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
        currentTime = 0;
      }
      else if (!(PlayerInput.Triggers.Current.Up || OriMod.ChargeKey.Current) && !Ending) {
        if (Active) {
          SetState(State.Ending);
        }
        else {
          SetState(State.Inactive);
        }
        currentTime = 0;
        return;
      }
      else if (Starting) {
        currentTime++;
        if (currentTime > StartDuration) {
          SetState(State.Active);
          currentTime = 0;
        }
      }
      else if (Ending) {
        currentTime++;
        if (currentTime > EndDuration) {
          SetState(State.Inactive);
          currentTime = 0;
        }
      }
    }
  }
}
