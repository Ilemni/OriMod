using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for crouching. No functional use.
  /// </summary>
  public sealed class Crouch : Ability {
    internal Crouch(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Crouch;

    internal override bool UpdateCondition => InUse || oPlayer.Input(PlayerInput.Triggers.Current.Down);
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Manager.lookUp.InUse && !Manager.dash.InUse && !Manager.chargeDash.InUse && !Restricted;
    private bool Restricted => OriMod.ConfigClient.SoftCrouch && (player.controlLeft || player.controlRight);
    private int StartDuration => 10;
    private int EndDuration => 4;

    protected override void UpdateUsing() {
      if (!OriMod.ConfigClient.SoftCrouch) {
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.velocity.X = 0;
        if (PlayerInput.Triggers.JustPressed.Left) {
          player.controlLeft = false;
          player.direction = -1;
        }
        else if (PlayerInput.Triggers.JustPressed.Right) {
          player.controlRight = false;
          player.direction = 1;
        }
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
        if (CanUse && PlayerInput.Triggers.Current.Down) {
          SetState(State.Starting);
          CurrentTime = 0;
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
        CurrentTime = 0;
      }
      else if (!PlayerInput.Triggers.Current.Down && !Ending) {
        if (Active) {
          SetState(State.Ending);
        }
        else {
          SetState(State.Inactive);
        }
        CurrentTime = 0;
        return;
      }
      else if (Starting) {
        CurrentTime++;
        if (CurrentTime > StartDuration) {
          SetState(State.Active);
          CurrentTime = 0;
        }
      }
      else if (Ending) {
        CurrentTime++;
        if (CurrentTime > EndDuration) {
          SetState(State.Inactive);
          CurrentTime = 0;
        }
      }
    }
  }
}
