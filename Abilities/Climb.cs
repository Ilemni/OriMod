using System;
using System.IO;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for climbing on walls.
  /// </summary>
  public sealed class Climb : Ability {
    internal Climb(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Climb;

    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !player.mount.Active && !abilities.wallJump.InUse && !abilities.wallChargeJump.InUse;

    internal bool IsCharging => Active && (wallDirection == 1 && player.controlLeft || wallDirection == -1 && player.controlRight);

    internal sbyte wallDirection;

    protected override void UpdateActive() {
      if (CurrentTime == 0) {
        wallDirection = (sbyte)player.direction;
      }

      if (IsCharging) {
        player.velocity.Y = 0;
      }
      else {
        if (player.controlUp) {
          player.velocity.Y += player.velocity.Y < (player.gravDir > 0 ? -2 : 4) ? 1 : -1;
        }
        else if (player.controlDown) {
          player.velocity.Y += player.velocity.Y < (player.gravDir > 0 ? 4 : -2) ? 1 : -1;
        }
        if (!player.controlDown && !player.controlUp) {
          player.velocity.Y *= Math.Abs(player.velocity.Y) > 1 ? 0.35f : 0;
        }
      }

      player.gravity = 0;
      player.runAcceleration = 0;
      player.maxRunSpeed = 0;
      player.direction = wallDirection;
      player.velocity.X = 0;
      player.controlLeft = false;
      player.controlRight = false;
      player.controlUp = false;
    }

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && IsLocal && OriMod.ClimbKey.Current) {
          SetState(State.Active);
        }
      }
      else if (!CanUse || IsLocal && !OriMod.ClimbKey.Current) {
        SetState(State.Inactive);
      }
    }
  }
}
