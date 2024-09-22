using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Projectiles.Abilities;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick and high jump that can deal damage to enemies.
/// </summary>
public sealed class ChargeJump(Player player) : OriAbility(player) {
  /// <summary>
  /// Coyote jump, how long to allow CJumping when no longer valid to do so.
  /// </summary>
  private static int MaxGrace => 25;

  private static int MaxCharge => 35;

  private static readonly float[] Speeds = [
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f,
    12f
  ];

  private static int Duration => Speeds.Length;


  private int _currentCharge;
  private int _currentGrace;

  private SoundInfo _chargeSound = new("Ori/ChargeJump/seinChargeJumpChargeB", 0, 0.6f);
  private SoundInfo _unchargeSound = new("Ori/ChargeDash/seinChargeDashUncharge", 0, 0.6f);
  private SoundInfo _startSound = new("Ori/ChargeJump/seinChargeJumpJump", 3, 1);


  private bool CanCharge => base.CanEnter() && Input.Charge.Current;

  public override int MaxLevel => 5;

  public override int MaxCooldown => 120;

  protected override bool StartCooldownOnEnter => true;


  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();
    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<LookUp>(to: this);
  }

  public override bool CanEnter() => base.CanEnter() && _currentCharge >= MaxCharge;

  protected override void OnEnter(State? fromState) {
    _startSound.Play(Player);
    _currentCharge = 0;
    Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
      ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, Player.whoAmI, 0, 1);
  }

  protected override bool OnPreUpdateInterruptible(State activeState) {
    // Update _currentCharge
    if (CanCharge) {
      if (_currentCharge < MaxCharge) {
        _currentCharge++;
        if (_currentCharge == 1 || _currentCharge == MaxCharge) {
          _chargeSound.Play(Player);
        }
      }
    }
    else if (_currentGrace <= 0 && _currentCharge > 0) {
      _currentCharge--;
    }

    if (_currentCharge < MaxCharge) {
      return false;
    }

    if (Input.Charge.Current && Input.Jump.JustPressed) {
      return true;
    }

    ChargeDust();

    // Update _currentGrace
    if (IsGrounded && CanCharge) {
      _currentGrace = MaxGrace;
    }
    else if (_currentGrace > 0) {
      _currentGrace--;
      if (_currentGrace == 0) {
        _unchargeSound.Play(Player);
      }
    }

    return false;
  }

  private void ChargeDust() {
    if (Main.rand.NextFloat() < 0.7f) {
      Dust.NewDust(Player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
    }
  }

  protected override void OnPreUpdate() {
    if (ActiveTime >= Duration) {
      CancelState();
    }
  }

  protected override void OnUpdate() {
    Player.controlJump = false;
    float speed = Speeds[ActiveTime] * 0.35f;
    Player.velocity.Y = speed * -Player.gravDir;
    OriPlayer.SetImmune(12);

    NetUpdate = true;
  }

  protected override bool CanRefresh(bool cooledDown) {
    return GetParent<MovementStates>().ActiveChild is Burrow || cooledDown;
  }

  protected override void OnEndCooldown() {
    RefreshParticles(Color.Blue);
    _currentCharge = 0;
    _currentGrace = 0;
  }

  protected override void NetSync(ISync sync) {
    sync.SyncPositionAndVelocity(Player);
  }

  protected override AnimationOptions? GetAnimationOptions() => new("ChargeJump");
}
