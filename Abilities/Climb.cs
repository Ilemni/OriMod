using System;
using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for climbing on walls.
  /// </summary>
  public sealed class Climb : Ability {
    internal Climb(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Climb;

    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !player.mount.Active && !Manager.wallJump.InUse && !Manager.wallChargeJump.InUse;

    internal bool IsCharging => Active && (wallDirection == 1 && PlayerInput.Triggers.Current.Left || wallDirection == -1 && PlayerInput.Triggers.Current.Right);

    internal sbyte wallDirection;

    private void StartClimb() {
      wallDirection = (sbyte)player.direction;
    }

    protected override void UpdateActive() {
      player.gravity = 0;
      player.runAcceleration = 0;
      player.maxRunSpeed = 0;
      player.direction = wallDirection;
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
          SetState(State.Active);
          StartClimb();
        }
      }
      else if (!CanUse || !OriMod.ClimbKey.Current) {
        SetState(State.Inactive);
      }
    }
  }
}
