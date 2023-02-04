using AnimLib.Abilities;
using OriMod.Utilities;
using Terraria;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for crouching. This ability is entirely visual, and is alwaays unlocked.
  /// </summary>
  public sealed class Crouch : Ability<OriAbilityManager> {
    public override int Id => AbilityId.Crouch;

    public override bool CanUse => base.CanUse && abilities.oPlayer.IsGrounded && !Restricted && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.dash && !abilities.launch &&
      !abilities.lookUp && !abilities.stomp;
    private bool Restricted => OriMod.ConfigClient.softCrouch && (player.controlLeft || player.controlRight);
    private static int StartDuration => 10;
    private static int EndDuration => 4;

    public override void UpdateUsing() {
      if (OriMod.ConfigClient.softCrouch) return;
      player.runAcceleration = 0;
      player.maxRunSpeed = 0;
      player.velocity.X = 0;
      if (player.controlLeft) {
        player.controlLeft = false;
        player.direction = -1;
      }
      else if (player.controlRight) {
        player.controlRight = false;
        player.direction = 1;
      }

      // if (PlayerInput.Triggers.JustPressed.Jump) { // TODO: Backflip
      //   Vector2 pos = player.position;
      //   pos = new Vector2(pos.X + 4, pos.Y + 52);
      //   pos.ToWorldCoordinates();
      //   if (!TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y].type] && !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y + 1].type]) {
      //     backflipping = true;
      //   }
      // }
    }

    public override void PreUpdate() {
      if (!InUse) {
        if (CanUse && player.controlDown) {
          SetState(AbilityState.Starting);
        }
      }
      else if (!CanUse) {
        SetState(AbilityState.Inactive);
      }
      else if (!player.controlDown && !Ending) {
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
            CameraControl.instance.camera_v_offset = OriMod.ConfigClient.lookUpCamOffset *
              (OriMod.ConfigClient.smoothCamera ? (OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f) : 1.0f) /
              Main.GameZoomTarget;
            break;
          default:
            if (abilities.lookUp.Inactive)
              CameraControl.instance.camera_v_offset = 0.0f;
            break;
        }
      }
    }
  }
}
