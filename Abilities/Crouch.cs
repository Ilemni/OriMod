using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Crouch : Ability {
    internal Crouch(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool DoUpdate => InUse || oPlayer.Input(OriPlayer.Current.Down); 
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Handler.lookUp.InUse && !Handler.dash.InUse && !Handler.cDash.InUse && !Restricted;
    private bool Restricted => OriMod.ConfigClient.SoftCrouch ? player.controlLeft || player.controlRight : false;
    private int StartDuration => 10;
    private int EndDuration => 4;
    
    private int CurrTime = 0;

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
          Starting = true;
          CurrTime = 0;
        }
      }
      else if (!CanUse) {
        Inactive = true;
        CurrTime = 0;
      }
      else if (!PlayerInput.Triggers.Current.Down && !Ending) {
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