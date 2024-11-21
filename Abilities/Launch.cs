using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
public sealed class Launch(Player player) : OriAbility(player) {
  public override int MaxLevel => 3;

  private static float NetAngleTolerance => 0.15f;
  private static float NetAngleLerpValue => 0.2f;

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();

    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<AirJump>(to: this);
    parent.AddInterruptible<Dash>(to: this);
    parent.AddInterruptible<Glide>(to: this);
    parent.AddInterruptible<WallJump>(to: this);
  }

  public override bool CanEnter() => base.CanEnter() && !IsGrounded;

  public bool Starting => _starting;

  private bool _starting;

  private int _currentChainTime;

  private ref LaunchStats Stats => ref IStats<LaunchStats>.Get(Level);

  private float _launchAngle;

  private Vector2 LaunchDirection => new((float)Math.Cos(_launchAngle), (float)Math.Sin(_launchAngle));

  private SoundInfo _endSound = new("Ori/Bash/seinBashEnd", 3, 0.55f);

  private int _currentChain;

  /// <summary>
  /// <see cref="_launchAngle"/> value sent across multiplayer clients.
  /// Not directly set to M<see cref="_launchAngle"/> to allow visual lerping
  /// </summary>
  private float _netAngle;

  private int _timeSinceLastSync;

  public override bool SupportsCooldown => true;

  protected override bool StartCooldownOnExit => true;


  protected override void NetSync(ISync sync) {
    sync.Sync7BitEncodedInt(ref _currentChain);
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

  protected override void OnUpdate() {
    if (_starting) {
      if (IsLocal) {
        OriUtils.GetMouseDirection(Player, out float angle, Vector2.One);
        _launchAngle = angle;

        // Determine whether to push a netupdate
        // Sync large changes, or small changes over larger time
        float angleDelta = Math.Abs(_launchAngle - _netAngle);
        if (
          (_timeSinceLastSync > 30 && angleDelta > 0.01f) ||
          (_timeSinceLastSync > 10 && angleDelta > NetAngleTolerance / 4) ||
          _timeSinceLastSync > 4 && angleDelta > NetAngleTolerance) {
          _netAngle = _launchAngle;
          NetUpdate = true;
          _timeSinceLastSync = 0;
        }
        else {
          _timeSinceLastSync++;
        }
      }
      else {
        // Non-local, smooth visual angle to net angle
        _launchAngle = LerpAngleRad(_launchAngle, _netAngle, NetAngleLerpValue);
      }

      Player.velocity *= 0.86f;
      Player.gravity = 0;
      Player.runSlowdown = 0;
    }
    else {
      Player.velocity = LaunchDirection * Stats.GetSpeed(_currentChain);
      OriPlayer.SetImmune(5);
    }

    // TODO: Find out how to actually cancel pulley
    Player.pulley = false;
    Player.maxFallSpeed = Stats.GetSpeed(_currentChain);

    // Allow only quick heal and quick mana
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
    BuffImmune();

    return;

    static float LerpAngleRad(float from, float to, float weight) {
      float num1 = (to - from) % MathF.Tau;
      float num2 = 2f * num1 % MathF.Tau - num1;
      return from + num2 * weight;
    }
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
    base.OnEnter(fromState);
    LaunchProjectile proj = NewAbilityProjectile<LaunchProjectile>(damage: 70);
    proj.Ability = this;

    SoundWrapper.PlayLocal(Player, "Ori/Bash/seinBashStartA", 0.5f);

    RestoreAirJumps();
    _currentChain = 1;
    _currentChainTime = 0;
    _starting = true;
  }

  protected override void OnExit() {
    Player.velocity = LaunchDirection * 10;
    _currentChainTime = 0;
    _starting = true;
  }

  protected override bool OnPreUpdateInterruptible(State activeState) {
    if (!IsLocal) {
      return false;
    }

    return CanEnter() && Input.Charge.Current && Input.Bash.JustPressed;
  }

  protected override void OnPreUpdate() {
    _currentChainTime++;

    if (IsGrounded || OnWall) {
      // Prevent any usage of Launch while not in air
      CancelState();
      return;
    }

    if (!IsLocal) {
      return;
    }

    ref LaunchStats stats = ref Stats;

    // Post-ending state depends on player input
    // Maybe too sensitive to rely on input packet
    if (_starting) {
      if (_currentChainTime > stats.GetMinDuration(_currentChain) && !Input.Bash.Current ||
          _currentChainTime > stats.GetMaxDuration(_currentChain)) {
        _currentChainTime = 0;
        _starting = false;
        NetUpdate = true;
      }

      return;
    }

    if (_currentChainTime <= stats.GetMovDuration(_currentChain)) {
      return;
    }

    if (_currentChain < stats.MaxChains && Input.Bash.Current) {
      _currentChain++;
      _currentChainTime = 0;
      _starting = true;
    }
    else {
      CancelState();
    }

    _endSound.Play(Player);
  }

  protected override bool CanRefresh(bool cooledDown) => _currentChain == 0;

  protected override void OnEndCooldown() {
    _currentChain = 0;
  }

  protected override AnimationOptions? GetAnimationOptions() {
    if (_starting) {
      // Somewhat accelerating speed of rotation
      float rotationSpeed =
        _currentChainTime * (_currentChainTime < 5 ? 0.05f : _currentChainTime < 20 ? 0.03f : 0.02f);
      rotationSpeed = float.Min(rotationSpeed, MathF.Tau * 0.3f);
      return new AnimationOptions("AirJump",
        rotationOffset: true,
        rotation: Player.direction * rotationSpeed);
    }

    // Launch angle needs to be offset by 90 degrees since it uses Stomp animation
    // Disable SpriteEffects as launching should not be flipped
    return new AnimationOptions("ChargeJump", speed: 0.67f,
      rotation: _launchAngle + (float)Math.PI / 2 * Player.gravDir, loopCount: 0, isPingPong: true,
      effects: SpriteEffects.None);
  }

  internal void GetDrawFields(AnimSpriteSheet sheet, out Vector2 position, out float rotation, out Rectangle rect) {
    position = OriPlayer.Player.Center;
    rotation = _launchAngle;
    rect = sheet.GetRectFromTimer("Launch", "Arrow", ActiveTime);
  }

  protected override void DebugText(DebugUIState ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelValue("Chain", _currentChain, Stats.MaxChains);
    ui.DrawAppendBoolean(Starting);
    ui.DrawAppendLabelValue(_currentChainTime, color: Color.LightGray);

    ui.DrawAppendLabelValue("Angle", _launchAngle, format:['F']);
    ui.DrawAppendLabelValue(_netAngle, color: Color.LightGray, format:['F']);
  }

  private readonly record struct LaunchStats(
    int MaxChains,
    int MinDuration,
    int MinChainedDuration,
    int MaxDuration,
    int MaxChainedDuration,
    int MovingDuration,
    int MovingMidDuration,
    float Speed,
    float ChainedSpeed
  ) : IStats<LaunchStats> {
    public static ref LaunchStats[] Values => ref _values;

    private static LaunchStats[] _values = [
      default,
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
        Speed: 40, ChainedSpeed: 45)
    ];

    public int GetMinDuration(int chain) => chain == 1 ? MinDuration : MinChainedDuration;
    public int GetMaxDuration(int chain) => chain == 1 ? MaxDuration : MaxChainedDuration;
    public int GetMovDuration(int chain) => chain == 1 || chain == MaxChains ? MovingDuration : MovingMidDuration;
    public float GetSpeed(int chain) => chain == 1 ? Speed : ChainedSpeed;

    public static LaunchStats CreateFromLevel(int level) {
      LaunchStats last = Values[^1];
      return last with {
        MaxChains = last.MaxChains + 3
      };
    }
  }
}
