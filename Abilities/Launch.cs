using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using System.Collections.Generic;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace OriMod.Abilities;

/// <summary>
/// Ability for launching the player in the desired direction. Used in the air.
/// </summary>
/// <remarks>
/// Seems a common trait that as an ability that is actually a leveled version of another ability.
/// Launch would be more fitting as a Charge Jump Lv3, rather than Bash Lv3.
/// This must be a separate class rather than built into Bash, as this would otherwise require
/// Bash to be unlocked as well to be usable.
/// </remarks>
public sealed class Launch : OriAbility {
  public override int MaxLevel => 2;

  private static float NetAngleTolerance => 0.15f;
  private static float NetAngleLerpValue => 0.2f;

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility.Jumping>(),
      GetState<NoAbility.Falling>(),
      GetState<AirJump>(),
      GetState<Dash>(),
      GetState<Glide>(),
      GetState<WallJump>()
    ]);
  }

  public override bool CanEnter() => base.CanEnter() && !IsGrounded && CurrentChain < Stats.MaxChains;

  public bool Starting => _starting;

  private bool _starting;

  private int _currentChainTime;

  public ref readonly LaunchStats Stats => ref IStats<LaunchStats>.Get(Level);

  private float _launchAngle;

  private Vector2 LaunchDirection => new(MathF.Cos(_launchAngle), MathF.Sin(_launchAngle));

  private SoundInfo _endSound = new("Ori/Bash/seinBashEnd", 3, 0.55f);

  public int CurrentChain;

  /// <summary>
  /// <see cref="_launchAngle"/> value sent across multiplayer clients.
  /// Not directly set to M<see cref="_launchAngle"/> to allow visual lerping
  /// </summary>
  private float _netAngle;

  private int _timeSinceLastSync;

  public override bool SupportsCooldown => true;

  protected override bool StartCooldownOnExit => CurrentChain >= Stats.MaxChains || !Stats.CanReEnter;

  public override bool ShowHintInUI => NPC.downedAncientCultist
    && GetState<ChargeJump>().Unlocked
    && GetState<ChargeDash>().Unlocked
    && GetState<WallChargeJump>().Unlocked;


  protected override void NetSync(NetSyncer sync) {
    sync.Sync7BitEncodedInt(ref CurrentChain);
    sync.Sync(ref _starting);
    sync.Sync(ref _netAngle);
    if (!_starting) {
      sync.SyncPositionAndVelocity(Player);
    }

    if (sync.Reading && Main.dedServ) {
      // Server needs to know actual value, and doesn't need visual lerping
      _launchAngle = _netAngle;
    }
  }

  public override void SetControls() {
    Player.controlJump = false;
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlHook = false;
    Player.controlInv = false;
    Player.controlMount = false;
    Player.controlSmart = false;
    Player.controlThrow = false;
    Player.controlTorch = false;
    Player.controlUseItem = false;
    Player.controlUseTile = false;
  }

  public override void PostUpdateRunSpeeds() {
    // TODO: Find out how to actually cancel pulley
    Player.pulley = false;
    Player.maxFallSpeed = Stats.GetSpeed(CurrentChain);

    BuffImmune();

    if (!_starting) {
      Player.velocity = LaunchDirection * Stats.GetSpeed(CurrentChain);
      return;
    }

    Player.velocity *= 0.86f;
    Player.gravity = 0;
    Player.runSlowdown = 0;

    if (!IsLocal) {
      // Non-local, smooth visual angle to net angle
      _launchAngle = _launchAngle.AngleLerp(_netAngle, NetAngleLerpValue);
      return;
    }

    OriUtils.GetMouseDirection(Player, out _launchAngle, Vector2.One);

    // Determine whether to push a netupdate
    // Sync large changes, or small changes over larger time
    if (ShouldNetUpdateAngle()) {
      _netAngle = _launchAngle;
      NetUpdate = true;
      _timeSinceLastSync = 0;
    }
    else {
      _timeSinceLastSync++;
    }
  }

  private bool ShouldNetUpdateAngle() {
    float angleDelta = Math.Abs(_launchAngle - _netAngle);
    return
      (_timeSinceLastSync > 30 && angleDelta > 0.01f) ||
      (_timeSinceLastSync > 10 && angleDelta > NetAngleTolerance / 4) ||
      _timeSinceLastSync > 4 && angleDelta > NetAngleTolerance;
  }

  private void BuffImmune() {
    Player.buffImmune.AssignValueToKeys(true, [
      BuffID.CursedInferno,
      BuffID.Dazed,
      BuffID.Frozen,
      BuffID.Frostburn,
      BuffID.MoonLeech,
      BuffID.Obstructed,
      BuffID.OnFire,
      BuffID.Poisoned,
      BuffID.ShadowFlame,
      BuffID.Silenced,
      BuffID.Slow,
      BuffID.Stoned,
      BuffID.Suffocation,
      BuffID.Venom,
      BuffID.Weak,
      BuffID.WitheredArmor,
      BuffID.WitheredWeapon,
      BuffID.WindPushed
    ]);
  }

  protected override void OnEnter(State? fromState) {
    NewAbilityProjectile<LaunchProjectile>(damage: 70);

    SoundWrapper.PlayLocal(Player, "OriMod/Sounds/Ori/Bash/seinBashStartA", 0.5f);

    // RestoreAirJumps would reset the current chain, so we need to save it
    int chain = CurrentChain;
    RestoreAirJumps();
    CurrentChain = ++chain;
    _currentChainTime = 0;
    _starting = true;
  }

  protected override void OnExit(State? toState) {
    Player.velocity = LaunchDirection * 10;
    _currentChainTime = 0;
    _starting = false;
  }

  protected override bool UpdateInterrupt(State activeState) {
    if (!IsLocal) {
      return false;
    }

    return CanEnter() && Input.Charge.Current && Input.Bash.JustPressed;
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) =>
    (Active && !Starting) || InactiveTime <= 5;

  public override void PostUpdateMiscEffects() {
    _currentChainTime++;

    if (IsGrounded || OnWall) {
      // Prevent any usage of Launch while not in air
      CancelState();
      return;
    }

    if (!IsLocal) {
      return;
    }

    ref readonly LaunchStats stats = ref Stats;

    // Post-ending state depends on player input
    // Maybe too sensitive to rely on input packet
    if (_starting) {
      if (_currentChainTime > stats.GetMinDuration(CurrentChain) && !Input.Bash.Current ||
          _currentChainTime > stats.GetMaxDuration(CurrentChain)) {
        _currentChainTime = 0;
        _starting = false;
        NetUpdate = true;
      }

      return;
    }

    if (_currentChainTime <= stats.GetMovDuration(CurrentChain)) {
      return;
    }

    if (CurrentChain < stats.MaxChains && Input.Bash.Current) {
      CurrentChain++;
      _currentChainTime = 0;
      _starting = true;
    }
    else {
      CancelState();
    }

    _endSound.Play(Player);
  }

  protected override bool CanRefresh(bool cooledDown) => IsGrounded || OnWall;

  protected override void OnEndCooldown(bool justCooledDown) {
    CurrentChain = 0;
  }

  public override AnimationOptions? GetAnimationOptions() {
    if (_starting) {
      // Somewhat accelerating speed of rotation
      float rotationSpeed =
        _currentChainTime * (_currentChainTime < 5 ? 0.05f : _currentChainTime < 20 ? 0.03f : 0.02f);
      return new AnimationOptions("AirJump") {
        RotationOffset = true,
        Rotation = Player.direction * Math.Min(rotationSpeed, MathF.Tau * 0.3f)
      };
    }

    // Launch angle needs to be offset by 90 degrees since it uses Stomp animation
    // Disable SpriteEffects as launching should not be flipped
    return new AnimationOptions("ChargeJump") {
      Speed = 0.67f,
      Rotation = _launchAngle + MathHelper.PiOver2 * Player.gravDir,
      LoopCount = 0,
      IsPingPong = true,
      Effects = SpriteEffects.None
    };
  }

  internal float GetRotation(ref readonly PlayerDrawSet drawInfo) {
    return !Character.UiInfo.IsDrawingInUI
      ? _launchAngle
      : (drawInfo.Position - Main.screenPosition).AngleTo(Main.MouseScreen);
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Chain", CurrentChain, Stats.MaxChains, Color.Blue);
    ui.DrawAppendBoolean(Starting);
    ui.DrawAppendLabelProgressBar("Chain Time (Start)", Starting ? _currentChainTime : 0,
      [Stats.GetMinDuration(CurrentChain), Stats.GetMaxDuration(CurrentChain)], Color.Blue);
    ui.DrawAppendLabelProgressBar("Chain Time", !Starting ? _currentChainTime : 0, Stats.GetMovDuration(CurrentChain),
      Color.Blue);

    ui.DrawAppendLabelValue("Angle", _launchAngle, format: ['F']);
    ui.DrawAppendLabelValue(_netAngle, Color.LightGray, format: ['F']);
  }

  protected override void DebugCooldownReason(UIStateInfo ui) {
    ui.DrawAppendLine("Must touch ground or wall to refresh.");
  }

  /// <param name="MaxChains">Max times the player can use Launch before requiring refresh (touch ground, etc.)</param>
  /// <param name="MinDuration">Minimum time to hold Launch key to activate.</param>
  /// <param name="MinChainedDuration"><paramref name="MinDuration"/>, but while mid- or end-chain.</param>
  /// <param name="MaxDuration">Maximum time the Launch key can be held before the ability will activate.</param>
  /// <param name="MaxChainedDuration"><paramref name="MaxDuration"/>, but while mid- or end-chain.</param>
  /// <param name="MovingDuration">Duration which Launch will be active, that is, moving the player.</param>
  /// <param name="MovingMidDuration"><paramref name="MovingDuration"/>, but while mid-chain. Not end-chain.</param>
  /// <param name="Speed">The velocity the player will be launched at.</param>
  /// <param name="ChainedSpeed"><paramref name="Speed"/>, but while mid- or end-chain.</param>
  /// <param name="CanReEnter">Whether the player can re-enter a partially used Launch without a refresh.</param>
  public readonly record struct LaunchStats(
    int MaxChains,
    int MinDuration,
    int MinChainedDuration,
    int MaxDuration,
    int MaxChainedDuration,
    int MovingDuration,
    int MovingMidDuration,
    float Speed,
    float ChainedSpeed,
    bool CanReEnter = false
  ) : IStats<LaunchStats> {
    public static LaunchStats[] Values { get; } = [
      new(MaxChains: 1,
        MinDuration: 15, MinChainedDuration: 20,
        MaxDuration: 45, MaxChainedDuration: 30,
        MovingDuration: 12, MovingMidDuration: 6,
        Speed: 25, ChainedSpeed: 40),
      new(MaxChains: 3,
        MinDuration: 15, MinChainedDuration: 20,
        MaxDuration: 45, MaxChainedDuration: 30,
        MovingDuration: 12, MovingMidDuration: 6,
        Speed: 25, ChainedSpeed: 40),
      new(MaxChains: 7,
        MinDuration: 8, MinChainedDuration: 11,
        MaxDuration: 20, MaxChainedDuration: 15,
        MovingDuration: 9, MovingMidDuration: 4,
        Speed: 40, ChainedSpeed: 45,
        CanReEnter: true)
    ];

    public int GetMinDuration(int chain) => chain == 1 ? MinDuration : MinChainedDuration;
    public int GetMaxDuration(int chain) => chain == 1 ? MaxDuration : MaxChainedDuration;
    public int GetMovDuration(int chain) => chain == 1 || chain == MaxChains ? MovingDuration : MovingMidDuration;
    public float GetSpeed(int chain) => chain == 1 ? Speed : ChainedSpeed;
  }
}
