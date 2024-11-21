using Microsoft.Xna.Framework;
using System;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick horizontal dash. May be used in the air.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing dashing with the Shield of Cthuhlu.
/// </remarks>
public sealed class Dash(Player player) : OriAbility(player) {
  public override int MaxLevel => 3;

  private static readonly float[] Speeds = [
    50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
    19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
  ];

  private static int Duration => Speeds.Length - 1;
  public override int MaxCooldown => Level >= 3 ? 0 : 60;

  private int _direction;
  private int _currentCount;
  private int MaxDashes => 1;

  private SoundInfo _startSound = new("Ori/Dash/seinDash", 3, 0.2f);

  private ChargeDash _chargeDash = null!; // OnInitialize()

  public override bool SupportsCooldown => true;

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();
    _chargeDash = parent.GetChild<ChargeDash>();
    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<Glide>(to: this);
    parent.AddInterruptible<WallJump>(to: this);
    parent.AddInterruptible<Crouch>(to: this);
    parent.AddInterruptible<LookUp>(to: this);
  }

  public override bool CanEnter() => base.CanEnter() && !OnWall && (Level >= 2 || IsGrounded);

  protected override void OnEnter(State? fromState) {
    _direction = (sbyte)(Player.controlLeft ? -1 : Player.controlRight ? 1 : Player.direction);
    _startSound.Play(Player);
    Player.pulley = false;
    _currentCount++;
    if (_currentCount >= MaxDashes) {
      StartCooldown();
    }
  }

  protected override void OnExit() {
    if (!OnWall) {
      Player.velocity.X = Math.Min(Speeds[^1], Math.Abs(Player.velocity.X)) * _direction; // Rip hyperspeed dash-jump
    }
  }

  protected override void NetSync(ISync sync) {
    sync.SyncSign(ref _direction);
    sync.Sync7BitEncodedInt(ref _currentCount);
    sync.SyncPositionAndVelocity(Player);
  }

  protected override bool OnPreUpdateInterruptible(State activeState) {
    return Input.Dash.JustPressed && (!Input.Charge.Current || !_chargeDash.CanEnter());
  }

  protected override void OnPreUpdate() {
    if (ActiveTime > Duration || OnWall) {
      CancelState();
    }
  }

  protected override void OnUpdate() {
    if (Player.controlJump && Player.AnyExtraJumpUsable()) {
      CancelState();
      return;
    }

    Player.velocity.X = Speeds[ActiveTime] * 0.5f * _direction;
    Player.velocity.Y = 0.25f * (ActiveTime + 1) * Player.gravDir;
    NetUpdate = true;
  }

  protected override bool CanRefresh(bool cooledDown) => _currentCount < MaxDashes || IsGrounded || Player.mount.Active;

  protected override void OnEndCooldown() {
    _currentCount = 0;
    RefreshParticles(Color.White);
  }


  protected override AnimationOptions? GetAnimationOptions() {
    int frameIndex = Math.Abs(Player.velocity.X) < 12f ? 1 : 0;
    return new AnimationOptions("Dash", frameIndex: frameIndex);
  }
}
