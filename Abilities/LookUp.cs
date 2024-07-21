using AnimLib.Abilities;
using System;

namespace OriMod.Abilities;

/// <summary>
/// Ability for looking up. Pairs with the ability <see cref="ChargeJump"/>.
/// <para>This ability on its own is entirely visual, and is always unlocked.</para>
/// </summary>
public sealed class LookUp : OriAbility {
  public override int Id => AbilityId.LookUp;

  public override bool CanUse => base.CanUse && IsGrounded && Math.Abs(Player.velocity.X) < 0.8f && !Player.mount.Active &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.ChargeDash && !Abilities.Climb && !Abilities.Crouch && !Abilities.Dash;

  private static int StartDuration => 12;
  private static int EndDuration => 8;

  public override void PreUpdate() {
    if (!InUse) {
      if (CanUse && (Player.controlUp || Abilities.oPlayer.Input.Charge.Current)) {
        SetState(AbilityState.Starting);
      }
    }
    else if (!CanUse) {
      SetState(AbilityState.Inactive);
    }
    else if (!(Player.controlUp || Abilities.oPlayer.Input.Charge.Current) && !Ending) {
      SetState(Active ? AbilityState.Ending : AbilityState.Inactive);
    }
    else if (Starting) {
      if (StateTime > StartDuration) {
        SetState(AbilityState.Active);
      }
    }
    else if (Ending) {
      if (StateTime > EndDuration) {
        SetState(AbilityState.Inactive);
      }
    }
  }
}
