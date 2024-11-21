using System;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for climbing on walls.
/// </summary>
public sealed class Climb(Player player) : OriAbility(player) {
  /// <summary>
  /// Time until <see cref="IsFullyCharged"/> becomes <see langword="true"/>.
  /// </summary>
  private static int MaxCharge => 35;

  /// <summary>
  /// Max angles which the player can aim for <see cref="WallChargeJump"/>.
  /// </summary>
  private static float MaxAimAngle => 0.65f;

  private WallChargeJump _wallChargeJump = null!; // OnInitialize()
  public override int MaxLevel => 1;

  /// <summary>
  /// Whether charging is possible. Requires <see cref="WallChargeJump"/> unlocked/not on cooldown.
  /// </summary>
  private bool CanCharge => !_wallChargeJump.IsOnCooldown && _isCharging && !Player.shimmering;

  /// <summary>
  /// Whether fully charged, that is, <see cref="WallChargeJump"/> can be transitioned into.
  /// </summary>
  internal bool IsFullyCharged => _currentCharge >= MaxCharge;


  /// <summary>
  /// Current charge progress.
  /// <see cref="IsFullyCharged"/> is <see langword="true"/> when this value reaches <see cref="MaxCharge"/>.
  /// </summary>
  private int _currentCharge;

  private Vector2 _chargeJumpAimDirection;

  private float _angle;

  private int _wallDirection;
  private int _gravDirection;

  // Prevent flip gravity when climbing upwards
  private bool _disableUp;

  internal bool Ending => _endingTime > 0;
  private int _endingTime;

  /// <summary>
  /// Whether the player is intending to charge (appropriate player.controlLeft/Right is true),
  /// and <see cref="WallChargeJump"/> is unlocked.
  /// </summary>
  private bool _isCharging;

  protected override void OnInitialize() {
    base.OnInitialize();
    _wallChargeJump = GetParent<MovementStates>().GetChild<WallChargeJump>();
  }

  public override bool CanEnter() => base.CanEnter() && OnWall && !IsGrounded;

  protected override void NetSync(ISync sync) {
    sync.Sync7BitEncodedInt(ref _currentCharge);
    sync.SyncSign(ref _wallDirection);
    sync.SyncSign(ref _gravDirection);
    sync.Sync(ref _isCharging);
    sync.Sync(ref _angle);

    if (sync.Reading) {
      _chargeJumpAimDirection = Vector2.UnitX.RotatedBy(_angle) * new Vector2(_wallDirection, _gravDirection);
    }
  }

  protected override void OnEnter(State? fromState) {
    _wallDirection = Player.direction;
    _gravDirection = (int)Player.gravDir;
  }

  protected override void OnExit() {
    _currentCharge = 0;
    _endingTime = 0;
  }

  protected override void OnPreUpdate() {
    if (!IsLocal) {
      return;
    }

    if (Player.controlDown) {
      CancelState();
      return;
    }

    bool prevIsCharging = _isCharging;
    _isCharging = _wallChargeJump.Unlocked && (_wallDirection == 1 ? Player.controlLeft : Player.controlRight);
    if (_isCharging != prevIsCharging) {
      NetUpdate = true;
    }


    if (!IsFullyCharged && CanCharge) {
      if (_currentCharge == 0) {
        SoundWrapper.PlayLocal(Player, "Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
      }

      _currentCharge++;
      if (IsFullyCharged) {
        NetUpdate = true;
        SoundWrapper.PlayLocal(Player, "Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
      }
    }

    if (IsFullyCharged) {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust.NewDust(Player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
      }

      Vector2 direction = new(-_wallDirection, _gravDirection);
      _chargeJumpAimDirection = OriUtils.GetMouseDirection(Player, out float angle, direction, MaxAimAngle);
      if (Math.Abs(_angle - angle) > 0.1f) {
        _angle = angle;
        NetUpdate = true;
      }

      if (!CanCharge) {
        NetUpdate = true;
        _currentCharge = 0;
        SoundWrapper.PlayLocal(Player, "Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
      }
    }

    if (Input.Jump.JustPressed) {
      if (IsFullyCharged && CanCharge) {
        _wallChargeJump.SetAimAndDirection(_angle, _chargeJumpAimDirection);
        TriggerState<WallChargeJump>();
        return;
      }

      if (TriggerState<WallJump>()) {
        return;
      }
    }

    if (Ending) {
      _endingTime++;
      int maxTime = Player.gravDir >= 1 ? 8 : 9;
      if (_endingTime >= maxTime) {
        CancelState();
      }
    }
    else {
      if (Input.Climb.Current && (CanEnter() || Player.controlUp || Input.Jump.Current)) {
        if (!CanEnter() && (Player.controlUp || Input.Jump.Current)) {
          // Climb over top of things
          _endingTime = 1;
        }
      }
      else {
        CancelState();
      }
    }
  }

  protected override void OnUpdate() {
    if (Player.controlUp) {
      _disableUp = true;
    }

    if (Ending) {
      // Clamber over ledge
      Player.velocity.X = _wallDirection * 3.7f;
      Player.velocity.Y = -_gravDirection * 4f;
      return;
    }

    if (_isCharging) {
      Player.velocity.Y = 0;
    }
    else if (Player.controlUp || Input.Jump.Current) {
      Player.velocity.Y += Player.velocity.Y < (Player.gravDir > 0 ? -2 : 4) ? 1 : -1;
    }
    else if (Player.controlDown) {
      Player.velocity.Y += Player.velocity.Y < (Player.gravDir > 0 ? 4 : -2) ? 1 : -1;
    }
    else {
      Player.velocity.Y *= Math.Abs(Player.velocity.Y) > 1 ? 0.35f : 0;
    }

    Player.gravity = 0;
    Player.jump = 0;
    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
    Player.ChangeDir(_wallDirection);
    Player.gravDir = _gravDirection;
    Player.velocity.X = 0;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlDown = false;
    Player.controlTorch = false;
    if (IsFullyCharged) {
      Player.controlUseItem = false;
    }
  }

  protected override void OnPostUpdate() {
    if (!IsActive) {
      return;
    }

    if (!_disableUp) {
      return;
    }

    if (!Player.controlUp) {
      _disableUp = false;
    }

    Player.controlUp = false;
  }

  protected override AnimationOptions? GetAnimationOptions() {
    if (Ending) {
      return new AnimationOptions("Jump", frameIndex: 0);
    }

    if (!_isCharging) {
      if (Math.Abs(Player.velocity.Y) < 0.1f) {
        return new AnimationOptions("ClimbIdle");
      }

      string tag = Player.velocity.Y * Player.gravDir < 0 ? "Climb" : "WallSlide";
      return new AnimationOptions(tag, speed: Math.Abs(Player.velocity.Y) * 0.4f);
    }

    if (!IsFullyCharged) {
      return new AnimationOptions("WallChargeJumpCharge", frameIndex: !_wallChargeJump.IsOnCooldown ? null : 0);
    }

    return new AnimationOptions("WallChargeJumpAim", frameIndex: _angle switch {
      < -0.46f => 2,
      < -0.17f => 1,
      > 0.46f => 4,
      > 0.17f => 3,
      _ => 0
    });
  }

  protected override void DebugText(DebugUIState ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelValue("X Direction", _wallDirection > 0 ? "Right" : "Left");
    ui.DrawAppendLabelValue("Y Direction", _gravDirection > 0 ? "Down" : "Up");
    if (_currentCharge > 0) {
      ui.DrawAppendBoolean(IsFullyCharged);
      ui.DrawAppendLabelValue("Current Charge", _currentCharge, MaxCharge);
      ui.DrawAppendLabelValue("Angle", _angle);
    }
  }
}
