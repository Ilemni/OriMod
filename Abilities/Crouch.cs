using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;

namespace OriMod.Abilities {
  public class Crouch : Ability {
    internal Crouch(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Handler.lookUp.InUse && !Handler.dash.InUse && !Handler.cDash.InUse;
    private const int StartDuration = 10;
    private const int EndDuration = 4;
    private int CurrTime = 0;

    protected override void UpdateUsing() {
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
      if (InUse) {
        if (!CanUse) {
          State = States.Inactive;
          CurrTime = 0;
          return;
        }
        if (!PlayerInput.Triggers.Current.Down && State != States.Ending) {
          State = State == States.Active ? States.Ending : States.Inactive;
          CurrTime = 0;
          return;
        }
        CurrTime++;
        if (State == States.Starting) {
          if (CurrTime > StartDuration) {
            State = States.Active;
            CurrTime = 0;
          }
        }
        else if (State == States.Ending) {
          if (CurrTime > EndDuration) {
            State = States.Inactive;
            CurrTime = 0;
          }
        }
      }
      else {
        if (CanUse && PlayerInput.Triggers.Current.Down) {
          State = States.Starting;
          CurrTime = 0;
        }
      }
    }
  }
}