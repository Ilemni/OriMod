using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System.IO;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping off walls.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing wall jumps with some accessories.
  /// </remarks>
  public sealed class WallJump : Ability<OriAbilityManager>, ILevelable {
    public override int Id => AbilityId.WallJump;
    public override int Level => (this as ILevelable).Level;
    int ILevelable.Level { get; set; }
    int ILevelable.MaxLevel => 1;

    public override bool CanUse => base.CanUse && abilities.oPlayer.OnWall &&
      !abilities.oPlayer.IsGrounded && !InUse && !player.mount.Active &&
      !abilities.wallChargeJump.Charged;

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private static int EndTime => 12;

    private sbyte _wallDirection;
    private sbyte _gravDirection;

    private readonly RandomChar _rand = new RandomChar();

    public override void ReadPacket(BinaryReader r) {
      _wallDirection = r.ReadSByte();
      _gravDirection = r.ReadSByte();
    }

    public override void WritePacket(ModPacket packet) {
      packet.Write(_wallDirection);
      packet.Write(_gravDirection);
    }

    public override void UpdateActive() {
      player.velocity.Y = WallJumpVelocity.Y * _gravDirection;
      abilities.oPlayer.PlaySound("Ori/WallJump/seinWallJumps" + _rand.NextNoRepeat(5), 0.75f);
    }

    public override void UpdateEnding() {
      if (abilities.oPlayer.OnWall) {
        player.velocity.Y -= _gravDirection;
      }
    }

    public override void UpdateUsing() {
      player.velocity.X = WallJumpVelocity.X * -_wallDirection;
      player.direction = _wallDirection;
      abilities.oPlayer.UnrestrictedMovement = true;
    }

    public override void PreUpdate() {
      if (CanUse && abilities.oPlayer.input.jump.JustPressed) {
        SetState(AbilityState.Active);
        if (IsLocal) {
          _wallDirection = (sbyte)player.direction;
          _gravDirection = (sbyte)player.gravDir;
        }
        abilities.climb.SetState(AbilityState.Inactive);
      }
      else if (Active) {
        SetState(AbilityState.Ending);
      }
      else if (Ending) {
        if (abilities.oPlayer.IsGrounded || stateTime > EndTime ||
          stateTime > EndTime * 0.5f && (player.controlRight || player.controlLeft)) {
          SetState(AbilityState.Inactive);
        }
      }
    }
  }
}
