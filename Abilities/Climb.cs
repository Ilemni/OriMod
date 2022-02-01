using System;
using System.IO;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for climbing on walls.
  /// </summary>
  public sealed class Climb : Ability, ILevelable {
    internal Climb(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityId.Climb;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 1;

    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.launch && !abilities.stomp && !abilities.wallChargeJump && !abilities.wallJump;

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

    protected override void ReadPacket(BinaryReader r) {
      wallDirection = r.ReadSByte();
      IsCharging = r.ReadBoolean();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(wallDirection);
      packet.Write(IsCharging);
    }

    protected override void UpdateActive() {
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
      player.controlDown = false;
    }

    protected override void UpdateEnding() {
      player.velocity.X = wallDirection * 5f;
      player.velocity.Y = -player.gravDir * 4f;
    }

    protected override void UpdateUsing() {
      if (player.controlUp) {
        _disableUp = true;
      }
    }

    protected internal override void PostUpdateAbilities() {
      if (!_disableUp) return;
      if (!player.controlUp) {
        _disableUp = false;
      }
      player.controlUp = false;
    }

    internal override void Tick() {
      if (!IsLocal) {
        return;
      }
      if (!InUse) {
        if (CanUse && input.climb.Current) {
          SetState(State.Active);
          wallDirection = (sbyte)player.direction;
        }
      }
      else if (Ending) {
        int maxTime = player.gravDir >= 1 ? 7 : 9;
        if (CurrentTime >= maxTime) {
          SetState(State.Inactive);
        }
      }
      else if (!input.climb.Current || !CanUse && !player.controlUp) {
        SetState(State.Inactive);
      }
      else if (!CanUse && player.controlUp) {
        // Climb over top of things
        SetState(State.Ending);
      }
      IsCharging = Active && abilities.wallChargeJump.Unlocked && (wallDirection == 1 ? player.controlLeft : player.controlRight);
    }
  }
}
