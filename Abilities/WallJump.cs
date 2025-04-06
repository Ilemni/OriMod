using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework;

namespace OriMod.Abilities;

/// <summary>
/// Ability for jumping off walls.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing wall jumps with some accessories.
/// </remarks>
public sealed class WallJump : OriAbility {
  private static readonly Vector2 WallJumpVelocity = new(4, -7.2f);

  private static int EndTime => 12;

  private int _wallDirection;
  private int _gravDirection;

  private SoundInfo _startSound = new("Ori/WallJump/seinWallJumps", 5, 0.75f);


  public override int MaxLevel => 1;

  public override bool ShowHintInUI => true;


  protected override bool CanTransitionFrom(State fromState) => fromState is not Climb { IsFullyCharged: true };

  public override bool CanEnter() => OnWall && !IsGrounded;

  protected override void OnEnter(State? fromState) {
    if (IsLocal) {
      NetUpdate = true;
      _wallDirection = Player.direction;
      _gravDirection = (int)Player.gravDir;
    }

    _startSound.Play(Player);
  }

  protected override void NetSync(NetSyncer sync) {
    bool doSync = ActiveTime == 0;
    sync.Sync(ref doSync);
    if (doSync) {
      sync.SyncSign(ref _wallDirection);
      sync.SyncSign(ref _gravDirection);
      sync.SyncPositionAndVelocity(Player);
    }
  }

  public override void PostUpdateMiscEffects() {
    if (IsGrounded || ActiveTime > EndTime ||
        (ActiveTime > EndTime * 0.5f && (Player.controlRight || Player.controlLeft))) {
      CancelState();
    }
  }

  public override void PostUpdateRunSpeeds() {
    Player.velocity.X = WallJumpVelocity.X * -_wallDirection;
    Player.ChangeDir(_wallDirection);
    if (ActiveTime == 0) {
      Player.velocity.Y = WallJumpVelocity.Y * _gravDirection;
    }
    else if (OnWall) {
      Player.velocity.Y -= _gravDirection;
    }
  }

  public override AnimationOptions? GetAnimationOptions() => new("WallJump");

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Duration", ActiveTime, EndTime, Color.Blue);
    ui.DrawAppendLabelValue("X Direction", _wallDirection > 0 ? "Right" : "Left");
    ui.DrawAppendLabelValue("Y Direction", _gravDirection > 0 ? "Down" : "Up");
  }
}
