using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Projectiles.Abilities;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick and high jump that can deal damage to enemies.
/// </summary>
public sealed class ChargeJump : OriAbility {
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
  private static int EndDuration => 12;


  private int _currentCharge;
  private int _currentGrace;

  private SoundInfo _chargeSound = new("Ori/ChargeJump/seinChargeJumpChargeB", 0, 0.6f);
  private SoundInfo _unchargeSound = new("Ori/ChargeDash/seinChargeDashUncharge", 0, 0.6f);
  private SoundInfo _startSound = new("Ori/ChargeJump/seinChargeJumpJump", 3, 1);


  private bool CanCharge => base.CanEnter() && Input.Charge.Current && IsGrounded;

  private bool Ending => ActiveTime >= Duration - EndDuration;

  public override int MaxLevel => 1;

  public override int MaxCooldown => 120;

  protected override bool StartCooldownOnEnter => true;

  public override bool ShowHintInUI =>
    Main.hardMode && (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3);

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility.Idle>(),
      GetState<NoAbility.IdleAgainst>(),
      GetState<NoAbility.Running>(),
      GetState<NoAbility.Falling>(),
      GetState<LookUp>()
    ]);
  }

  public override bool CanEnter() => base.CanEnter() && _currentCharge >= MaxCharge;

  protected override void OnEnter(State? fromState) {
    _startSound.Play(Player);
    _currentCharge = 0;
    _currentGrace = 0;
    Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
      ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, Player.whoAmI, 0, 1);
  }

  protected override bool UpdateInterrupt(State activeState) {
    // Update _currentCharge
    if (CanCharge) {
      if (_currentCharge < MaxCharge) {
        _currentCharge++;
        if (_currentCharge == 1 || _currentCharge == MaxCharge) {
          _chargeSound.Play(Player);
        }
      }
      else if (IsGrounded) {
        _currentGrace = MaxGrace;
      }
    }

    if (_currentCharge < MaxCharge) {
      return false;
    }

    if (Input.Charge.Current && Input.Jump.JustPressed) {
      return true;
    }

    ChargeDust();

    return false;
  }

  private void ChargeDust() {
    if (Main.rand.NextFloat() < 0.7f) {
      Dust.NewDust(Player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
    }
  }

  public override void SetControls() {
    Player.controlJump = false;
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => Active;

  public override void PostUpdateMiscEffects() {
    if (ActiveTime >= Duration) {
      CancelState();
    }
  }

  public override void PostUpdateRunSpeeds() {
    float speed = Speeds[ActiveTime] * 0.35f;
    Player.velocity.Y = speed * -Player.gravDir;

    NetUpdate = true;
  }

  public override void PostUpdate() {
    if (!CanCharge) {
      if (_currentGrace > 0) {
        _currentGrace--;
      }
      else if (_currentCharge > 0) {
        _currentCharge--;
        if (_currentCharge == 0) {
          _unchargeSound.Play(Player);
        }
      }
    }
  }

  protected override bool CanRefresh(bool cooledDown) {
    return cooledDown || GetState<Burrow>().Active;
  }

  protected override void OnEndCooldown(bool justCooledDown) {
    _currentCharge = 0;
    _currentGrace = 0;
    if (justCooledDown) {
      RefreshParticles(Color.Blue);
    }
  }

  protected override void NetSync(NetSyncer sync) {
    sync.SyncPositionAndVelocity(Player);
  }

  public override AnimationOptions? GetAnimationOptions() =>
    Ending && Anim.HasTag("ChargeJumpEnd")
      ? new AnimationOptions("ChargeJumpEnd") { LoopCount = 1 }
      : new AnimationOptions("ChargeJump");

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Duration", ActiveTime, Duration);
    ui.DrawAppendLabelProgressBar("Charge", _currentCharge, MaxCharge);
    ui.DrawAppendLabelProgressBar("Grace", _currentGrace, MaxGrace);
  }
}
