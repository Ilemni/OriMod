using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for jumping off walls.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing wall jumps with some accessories.
/// </remarks>
public sealed class WallJump : OriAbility, ILevelable {
  public override int Id => AbilityId.WallJump;
  public override int Level => ((ILevelable)this).Level;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 1;
  public override bool Unlocked => Level > 0;

  public override bool CanUse => base.CanUse && OnWall &&
    !IsGrounded && !InUse && !Player.mount.Active &&
    !Abilities.WallChargeJump.Charged;

  private static readonly Vector2 WallJumpVelocity = new(4, -7.2f);
  private static int EndTime => 12;

  private sbyte _wallDirection;
  private sbyte _gravDirection;

  private readonly RandomChar _rand = new();

  public override void ReadPacket(BinaryReader r) {
    _wallDirection = r.ReadSByte();
    _gravDirection = r.ReadSByte();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_wallDirection);
    packet.Write(_gravDirection);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }

  public override void UpdateActive() {
    Player.velocity.Y = WallJumpVelocity.Y * _gravDirection;
    PlaySound("Ori/WallJump/seinWallJumps" + _rand.NextNoRepeat(5), 0.75f);
  }

  public override void UpdateEnding() {
    if (OnWall) {
      Player.velocity.Y -= _gravDirection;
    }
  }

  public override void UpdateUsing() {
    Player.velocity.X = WallJumpVelocity.X * -_wallDirection;
    Player.direction = _wallDirection;
  }

  public override void PreUpdate() {
    if (CanUse && Input.Jump.JustPressed && IsLocal) {
      SetState(AbilityState.Active);
      if (IsLocal) {
        _wallDirection = (sbyte)Player.direction;
        _gravDirection = (sbyte)Player.gravDir;
      }
      Abilities.Climb.SetState(AbilityState.Inactive);
    }
    else if (Active) {
      SetState(AbilityState.Ending);
    }
    else if (Ending) {
      if (IsGrounded || StateTime > EndTime ||
          (StateTime > EndTime * 0.5f && (Player.controlRight || Player.controlLeft))) {
        SetState(AbilityState.Inactive);
      }
    }
  }
}
