using AnimLib.Abilities;
using OriMod.Utilities;
using System;
using Terraria;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for looking up. Pairs with the ability <see cref="ChargeJump"/>.
  /// <para>This ability on its own is entirely visual, and is always unlocked.</para>
  /// </summary>
  public sealed class LookUp : Ability<OriAbilityManager> {
    public override int Id => AbilityId.LookUp;

    public override bool CanUse => base.CanUse && abilities.oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !player.mount.Active &&
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

    public override void PostUpdateAbilities() {
      if (abilities.oPlayer.IsLocal) {
        switch (state) {
          case AbilityState.Active:
            CameraControl.instance.camera_v_offset = -OriMod.ConfigClient.lookUpCamOffset *
              (OriMod.ConfigClient.smoothCamera ? (OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f) : 1.0f) /
              Main.GameZoomTarget;
            break;
          default:
            if (abilities.crouch.Inactive)
              CameraControl.instance.camera_v_offset = 0.0f;
            break;
        }
      }
    }
  }
}
