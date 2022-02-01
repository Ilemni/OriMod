namespace OriMod.Abilities {
  /// <summary>
  /// Ability for crouching. This ability is entirely visual, and is alwaays unlocked.
  /// </summary>
  public sealed class Crouch : Ability {
    internal Crouch(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityId.Crouch;
    public override byte Level => 1;

    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Restricted && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.dash && !abilities.launch &&
      !abilities.lookUp && !abilities.stomp;
    private bool Restricted => OriMod.ConfigClient.softCrouch && (player.controlLeft || player.controlRight);
    private static int StartDuration => 10;
    private static int EndDuration => 4;

    protected override void UpdateUsing() {
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

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && player.controlDown) {
          SetState(State.Starting);
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
      }
      else if (!player.controlDown && !Ending) {
        SetState(Active ? State.Ending : State.Inactive);
      }
      else if (Starting) {
        if (CurrentTime > StartDuration) {
          SetState(State.Active);
        }
      }
      else if (Ending) {
        if (CurrentTime > EndDuration) {
          SetState(State.Inactive);
        }
      }
    }
  }
}
