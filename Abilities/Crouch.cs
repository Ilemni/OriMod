using AnimLib.Abilities;

namespace OriMod.Abilities;

/// <summary>
/// Ability for crouching. This ability is entirely visual, and is always unlocked.
/// </summary>
public sealed class Crouch : OriAbility {
  public override int Id => AbilityId.Crouch;

  public override bool CanUse => base.CanUse && IsGrounded && !Restricted && !Player.mount.Active &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.ChargeDash && !Abilities.Dash && !Abilities.Launch &&
    !Abilities.LookUp && !Abilities.Stomp;
  private bool Restricted => OriMod.ConfigClient.softCrouch && (Player.controlLeft || Player.controlRight);
  private static int StartDuration => 10;
  private static int EndDuration => 4;

  public override void UpdateUsing() {
    if (OriMod.ConfigClient.softCrouch) return;
    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
    Player.velocity.X = 0;
    if (Player.controlLeft) {
      Player.controlLeft = false;
      Player.direction = -1;
    }
    else if (Player.controlRight) {
      Player.controlRight = false;
      Player.direction = 1;
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
      if (CanUse && Player.controlDown) {
        SetState(AbilityState.Starting);
      }
    }
    else if (!CanUse) {
      SetState(AbilityState.Inactive);
    }
    else if (!Player.controlDown && !Ending) {
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
