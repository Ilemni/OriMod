using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping off walls.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraria already allowing wall jumps with some accessories.
  /// </remarks>
  public sealed class WallJump : Ability {
    internal WallJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.WallJump;

    internal override bool CanUse => base.CanUse && oPlayer.OnWall && !oPlayer.IsGrounded && !InUse && !player.mount.Active && !Manager.wallChargeJump.Charged;

    private static readonly Vector2 WallJumpVelocity = new Vector2(4, -7.2f);
    private int EndTime => 12;

    private sbyte wallDirection;
    private sbyte gravDirection;

    private readonly RandomChar rand = new RandomChar();

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
      if (CanUse && PlayerInput.Triggers.JustPressed.Jump) {
        SetState(State.Active);
        wallDirection = (sbyte)player.direction;
        gravDirection = (sbyte)player.gravDir;
        Manager.climb.SetState(State.Inactive);
      }
      else if (Active) {
        SetState(State.Ending);
      }
      else if (Ending) {
        if (CurrentTime > EndTime || PlayerInput.Triggers.JustPressed.Right || PlayerInput.Triggers.JustPressed.Left || oPlayer.IsGrounded) {
          SetState(State.Inactive);
        }
      }
    }
  }
}
