using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Crouch : Ability {
    internal Crouch(AbilityManager handler) : base(handler) { }
    public override int Id => AbilityID.Crouch;

    internal override bool DoUpdate => InUse || oPlayer.Input(PlayerInput.Triggers.Current.Down);
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Manager.lookUp.InUse && !Manager.dash.InUse && !Manager.cDash.InUse && !Restricted;
    private bool Restricted => OriMod.ConfigClient.SoftCrouch ? player.controlLeft || player.controlRight : false;
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
          CurrTime = 0;
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
        CurrTime = 0;
      }
      else if (!PlayerInput.Triggers.Current.Down && !Ending) {
        if (Active) {
          SetState(State.Ending);
        }
        else {
          SetState(State.Inactive);
        }
        CurrTime = 0;
        return;
      }
      else if (Starting) {
        CurrTime++;
        if (CurrTime > StartDuration) {
          SetState(State.Active);
          CurrTime = 0;
        }
      }
      else if (Ending) {
        CurrTime++;
        if (CurrTime > EndDuration) {
          SetState(State.Inactive);
          CurrTime = 0;
        }
      }
    }
  }
}
