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

  public override bool CanUse => base.CanUse && OnWall && !IsGrounded && !Player.mount.Active &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.Launch && !Abilities.Stomp && !Abilities.WallChargeJump &&
    !Abilities.WallJump;

  internal bool IsCharging {
    get => _isCharging;
    private set {
      if (value == _isCharging) return;
      _isCharging = value;
      NetUpdate = true;
    }
  }
  private bool _isCharging;

  internal sbyte WallDirection;
  // Prevent flip gravity when climbing upwards
  private bool _disableUp;

  public override void ReadPacket(BinaryReader r) {
    WallDirection = r.ReadSByte();
    IsCharging = r.ReadBoolean();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(WallDirection);
    packet.Write(IsCharging);
  }

  public override void UpdateActive() {
    if (IsCharging) {
      Player.velocity.Y = 0;
    }
    else if (Player.controlUp || Input.Jump.Current) {
      Player.velocity.Y += Player.velocity.Y < (Player.gravDir > 0 ? -2 : 4) ? 1 : -1;
    }
    else if (Player.controlDown) {
      Player.velocity.Y += Player.velocity.Y < (Player.gravDir > 0 ? 4 : -2) ? 1 : -1;
    }
    else {
      Player.velocity.Y *= Math.Abs(Player.velocity.Y) > 1 ? 0.35f : 0;
    }

    Player.gravity = 0;
    Player.jump = 0;
    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
    Player.direction = WallDirection;
    Player.velocity.X = 0;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlDown = false;
  }

  public override void UpdateEnding() {
    Player.velocity.X = WallDirection * 3f;
    Player.velocity.Y = -Player.gravDir * 4f;
  }

  public override void UpdateUsing() {
    if (Player.controlUp) {
      _disableUp = true;
    }
  }

  public override void PostUpdateAbilities() {
    if (!_disableUp) return;
    if (!Player.controlUp) {
      _disableUp = false;
    }
    Player.controlUp = false;
  }

  public override void PreUpdate() {
    if (!IsLocal) {
      return;
    }
    if (!InUse) {
      if (CanUse && Input.Climb.Current) {
        SetState(AbilityState.Active);
        WallDirection = (sbyte)Player.direction;
      }
    }
    else if (Ending) {
      int maxTime = Player.gravDir >= 1 ? 7 : 9;
      if (StateTime >= maxTime) {
        SetState(AbilityState.Inactive);
      }
    }
    else if (!Input.Climb.Current || (!CanUse && !(Player.controlUp || Input.Jump.Current))) {
      SetState(AbilityState.Inactive);
    }
    else if (!CanUse && (Player.controlUp || Input.Jump.Current)) {
      // Climb over top of things
      SetState(AbilityState.Ending);
    }
    IsCharging = Active && Abilities.WallChargeJump.Unlocked && (WallDirection == 1 ? Player.controlLeft : Player.controlRight);
  }
}
