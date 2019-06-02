using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Climb : Ability {
    internal Climb(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }

    private int WallDir = 0;
    internal override bool CanUse {
      get {
        return oPlayer.OnWall;
      }
    }
    private void StartClimb() {
      WallDir = player.direction;
    }
    
    protected override void UpdateActive() {
      player.gravity = 0;
      player.runAcceleration = 0;
      player.maxRunSpeed = 0;
      player.direction = WallDir;
      player.velocity.X = 0;
      player.controlLeft = false;
      player.controlRight = false;

      if (PlayerInput.Triggers.Current.Up) {
        player.velocity.Y += player.velocity.Y < -2 ? 1 : -1;
      }
      else if (PlayerInput.Triggers.Current.Down) {
        player.velocity.Y += player.velocity.Y < 4 ? 1 : -1;
      }
      if (
        (player.velocity.Y > 1 && !PlayerInput.Triggers.Current.Down) ||
        (player.velocity.Y < 1 && !PlayerInput.Triggers.Current.Up)) {
        player.velocity.Y /= 3;
      }
      if (
        player.velocity.Y > -1 && !PlayerInput.Triggers.Current.Down &&
        player.velocity.Y < 1 && !PlayerInput.Triggers.Current.Up
      ) {
        player.velocity.Y = 0;
      }
      if ((WallDir == 1 && PlayerInput.Triggers.Current.Left) || (WallDir == -1 && PlayerInput.Triggers.Current.Right)) {
        player.velocity.Y = 0;
      }
    }

    internal override void Tick() {
      if (InUse) {
        if (!oPlayer.OnWall) {
          State = States.Inactive;
        }
      }
      else {
        if (CanUse && OriMod.ClimbKey.Current) {
          State = States.Active;
          StartClimb();
        }
      }
    }
  }
}