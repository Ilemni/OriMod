using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Climb : Ability {
    internal Climb(OriAbilities handler) : base(handler) { }
    public override int id => AbilityID.Climb;

    internal override bool DoUpdate => oPlayer.Input(OriMod.ClimbKey.Current) && oPlayer.OnWall;
    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !player.mount.Active && !Handler.wJump.InUse && !Handler.wCJump.InUse;

    internal bool IsCharging => Active && (WallDir == 1 && PlayerInput.Triggers.Current.Left || WallDir == -1 && PlayerInput.Triggers.Current.Right);

    internal int WallDir;

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
      player.controlUp = false;

      if (IsCharging) {
        player.velocity.Y = 0;
      }
      else {
        if (PlayerInput.Triggers.Current.Up) {
          player.velocity.Y += player.velocity.Y < (player.gravDir > 0 ? -2 : 4) ? 1 : -1;
        }
        else if (PlayerInput.Triggers.Current.Down) {
          player.velocity.Y += player.velocity.Y < (player.gravDir > 0 ? 4 : -2) ? 1 : -1;
        }
        if (!PlayerInput.Triggers.Current.Down && !PlayerInput.Triggers.Current.Up) {
          player.velocity.Y *= Math.Abs(player.velocity.Y) > 1 ? 0.35f : 0;
        }
      }
    }

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && OriMod.ClimbKey.Current) {
          Active = true;
          StartClimb();
        }
      }
      else if (!CanUse || !OriMod.ClimbKey.Current) {
        Inactive = true;
      }
    }
  }
}
