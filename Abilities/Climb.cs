using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Climb : Ability {
    internal Climb(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }

    internal int WallDir = 0;
    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !player.mount.Active && !Handler.wJump.InUse;
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
        (!PlayerInput.Triggers.Current.Down) && !PlayerInput.Triggers.Current.Up) {
        player.velocity.Y *= Math.Abs(player.velocity.Y) > 1 ? 0.35f : 0;
      }
      if ((WallDir == 1 && PlayerInput.Triggers.Current.Left) || (WallDir == -1 && PlayerInput.Triggers.Current.Right)) {
        player.velocity.Y = 0;
      }
    }

    internal override void Tick() {
      if (InUse) {
        if (!oPlayer.OnWall || !OriMod.ClimbKey.Current) {
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