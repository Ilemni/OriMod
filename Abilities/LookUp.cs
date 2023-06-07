using AnimLib.Abilities;
using System;

namespace OriMod.Abilities; 

/// <summary>
/// Ability for looking up. Pairs with the ability <see cref="ChargeJump"/>.
/// <para>This ability on its own is entirely visual, and is always unlocked.</para>
/// </summary>
public sealed class LookUp : OriAbility {
  public override int Id => AbilityId.LookUp;

  public override bool CanUse => base.CanUse && IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !player.mount.Active &&
    !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.climb && !abilities.crouch && !abilities.dash;

  private static int StartDuration => 12;
  private static int EndDuration => 8;

  public override void PreUpdate() {
    if (!InUse) {
      if (CanUse && (player.controlUp || abilities.oPlayer.input.charge.Current)) {
        SetState(AbilityState.Starting);
      }
    }
    else if (!CanUse) {
      SetState(AbilityState.Inactive);
    }
    else if (!(player.controlUp || abilities.oPlayer.input.charge.Current) && !Ending) {
      SetState(Active ? AbilityState.Ending : AbilityState.Inactive);
    }
    else if (Starting) {
      if (stateTime > StartDuration) {
        SetState(AbilityState.Active);
      }
    }
    else if (Ending) {
      if (stateTime > EndDuration) {
        SetState(AbilityState.Inactive);
      }
    }
  }
}
