using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping off walls.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing wall jumps with some accessories.
  /// </remarks>
  public sealed class WallJump : Ability, ILevelable {
    internal WallJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.WallJump;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 1;

    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !InUse && !player.mount.Active && !abilities.wallChargeJump.Charged;

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private static int EndTime => 12;

    private sbyte wallDirection;
    private sbyte gravDirection;

    private readonly RandomChar rand = new RandomChar();

    protected override void ReadPacket(BinaryReader r) {
      wallDirection = r.ReadSByte();
      gravDirection = r.ReadSByte();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(wallDirection);
      packet.Write(gravDirection);
    }

    protected override void UpdateActive() {
      player.velocity.Y = WallJumpVelocity.Y * gravDirection;
      oPlayer.PlayNewSound("Ori/WallJump/seinWallJumps" + rand.NextNoRepeat(5));
    }

    protected override void UpdateEnding() {
      if (oPlayer.OnWall) {
        player.velocity.Y -= gravDirection;
      }
    }

    protected override void UpdateUsing() {
      player.velocity.X = WallJumpVelocity.X * -wallDirection;
      player.direction = wallDirection;
      oPlayer.UnrestrictedMovement = true;
    }

    internal override void Tick() {
      if (CanUse && oPlayer.justPressedJumped) {
        SetState(State.Active);
        if (IsLocal) {
          wallDirection = (sbyte)player.direction;
          gravDirection = (sbyte)player.gravDir;
        }
        abilities.climb.SetState(State.Inactive);
      }
      else if (Active) {
        SetState(State.Ending);
      }
      else if (Ending) {
        if (oPlayer.IsGrounded || CurrentTime > EndTime || CurrentTime > EndTime * 0.5f && (player.controlRight || player.controlLeft)) {
          SetState(State.Inactive);
        }
      }
    }
  }
}
