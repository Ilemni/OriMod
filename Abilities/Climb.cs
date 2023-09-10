using AnimLib.Abilities;
using System;
using System.IO;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for climbing on walls.
/// </summary>
public sealed class Climb : OriAbility, ILevelable {
  public override int Id => AbilityId.Climb;
  public override int Level => ((ILevelable)this).Level;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 1;
  public override bool Unlocked => Level > 0;

  public override bool CanUse => base.CanUse && OnWall && !IsGrounded && !player.mount.Active &&
    !abilities.bash && !abilities.burrow && !abilities.launch && !abilities.stomp && !abilities.wallChargeJump &&
    !abilities.wallJump;

  internal bool IsCharging {
    get => _isCharging;
    private set {
      if (value == _isCharging) return;
      _isCharging = value;
      netUpdate = true;
    }
  }
  private bool _isCharging;

  internal sbyte wallDirection;
  // Prevent flip gravity when climbing upwards
  private bool _disableUp;

  public override void ReadPacket(BinaryReader r) {
    wallDirection = r.ReadSByte();
    IsCharging = r.ReadBoolean();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(wallDirection);
    packet.Write(IsCharging);
  }

  public override void UpdateActive() {
    if (IsCharging) {
      player.velocity.Y = 0;
    }
    else if (player.controlUp || input.jump.Current) {
      player.velocity.Y += player.velocity.Y < (player.gravDir > 0 ? -2 : 4) ? 1 : -1;
    }
    else if (player.controlDown) {
      player.velocity.Y += player.velocity.Y < (player.gravDir > 0 ? 4 : -2) ? 1 : -1;
    }
    else {
      player.velocity.Y *= Math.Abs(player.velocity.Y) > 1 ? 0.35f : 0;
    }

    player.gravity = 0;
    player.jump = 0;
    player.runAcceleration = 0;
    player.maxRunSpeed = 0;
    player.direction = wallDirection;
    player.velocity.X = 0;
    player.controlLeft = false;
    player.controlRight = false;
    player.controlDown = false;
  }

  public override void UpdateEnding() {
    player.velocity.X = wallDirection * 3f;
    player.velocity.Y = -player.gravDir * 4f;
  }

  public override void UpdateUsing() {
    if (player.controlUp) {
      _disableUp = true;
    }
  }

  public override void PostUpdateAbilities() {
    if (!_disableUp) return;
    if (!player.controlUp) {
      _disableUp = false;
    }
    player.controlUp = false;
  }

  public override void PreUpdate() {
    if (!IsLocal) {
      return;
    }
    if (!InUse) {
      if (CanUse && input.climb.Current) {
        SetState(AbilityState.Active);
        wallDirection = (sbyte)player.direction;
      }
    }
    else if (Ending) {
      int maxTime = player.gravDir >= 1 ? 7 : 9;
      if (stateTime >= maxTime) {
        SetState(AbilityState.Inactive);
      }
    }
    else if (!input.climb.Current || (!CanUse && !(player.controlUp || input.jump.Current))) {
      SetState(AbilityState.Inactive);
    }
    else if (!CanUse && (player.controlUp || input.jump.Current)) {
      // Climb over top of things
      SetState(AbilityState.Ending);
    }
    IsCharging = Active && abilities.wallChargeJump.Unlocked && (wallDirection == 1 ? player.controlLeft : player.controlRight);
  }
}
