using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// Ability for jumping off walls.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing wall jumps with some accessories.
/// </remarks>
public sealed class WallJump(Player player) : OriAbility(player) {
  private static readonly Vector2 WallJumpVelocity = new(4, -7.2f);

  private static int EndTime => 12;

  private int _wallDirection;
  private int _gravDirection;

  private SoundInfo _startSound = new("Ori/WallJump/seinWallJumps", 5, 0.75f);


  public override int MaxLevel => 1;


  protected override bool CanTransitionFrom(State fromState) => fromState is not Climb climb || climb.IsFullyCharged;

  public override bool CanEnter() => OnWall && !IsGrounded;

  protected override void OnEnter(State? fromState) {
    if (IsLocal) {
      _wallDirection = Player.direction;
      _gravDirection = (int)Player.gravDir;
    }

    _startSound.Play(Player);
  }

  protected override void NetSync(ISync sync) {
    sync.SyncSign(ref _wallDirection);
    sync.SyncSign(ref _gravDirection);
    sync.SyncPositionAndVelocity(Player);
  }

  protected override void OnPreUpdate() {
    if (IsGrounded || ActiveTime > EndTime ||
        (ActiveTime > EndTime * 0.5f && (Player.controlRight || Player.controlLeft))) {
      CancelState();
    }
  }

  protected override void OnUpdate() {
    Player.velocity.X = WallJumpVelocity.X * -_wallDirection;
    Player.ChangeDir(_wallDirection);
    if (ActiveTime == 0) {
      Player.velocity.Y = WallJumpVelocity.Y * _gravDirection;
    }
    else if (OnWall) {
      Player.velocity.Y -= _gravDirection;
    }
  }

  protected override AnimationOptions? GetAnimationOptions() => new("WallJump");
}
