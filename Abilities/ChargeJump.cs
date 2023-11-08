using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick and high jump that can deal damage to enemies.
/// </summary>
public sealed class ChargeJump : OriAbility, ILevelable {
  public override bool Unlocked => Level > 0;
  public override int Id => AbilityId.ChargeJump;
  public override int Level => ((ILevelable)this).Level;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 5;

  public override bool CanUse => base.CanUse && !InUse && Charged &&
    !abilities.burrow && !abilities.climb && !abilities.launch &&
    !abilities.stomp && !abilities.wallChargeJump;

  public override int Cooldown => 120;
  public override void OnRefreshed() => abilities.RefreshParticles(Color.Blue);

  private bool CanCharge => base.CanUse && input.charge.Current;
  public bool Charged => _currentCharge >= MaxCharge;
  public bool Grace => _currentGrace > 0;

  /// <summary>
  /// Coyote jump, how long to allow CJumping when no longer valid to do so.
  /// </summary>
  private static int MaxGrace => 25;

  private static int MaxCharge => 35;

  private static float[] Speeds => _speeds ??= Unloadable.New(new[] {
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f,
    12f
  }, () => _speeds = null);

  private static float[] _speeds;
  private static int Duration => Speeds.Length;

  private int _currentCharge;
  private int _currentGrace;

  private readonly RandomChar _rand = new();

  private void StartChargeJump() {
    PlaySound("Ori/ChargeJump/seinChargeJumpJump" + _rand.NextNoRepeat(3));
    _currentCharge = 0;
    Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero,
      ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, player.whoAmI, 0, 1);
    StartCooldown();
    abilities.climb.SetState(AbilityState.Inactive);
  }

  private void UpdateCharged() {
    if (Main.rand.NextFloat() < 0.7f) {
      Dust.NewDust(player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
    }
  }

  public override void UpdateActive() {
    float speed = Speeds[stateTime] * 0.35f;
    player.velocity.Y = speed * -player.gravDir;
    oPlayer.immuneTimer = 12;

    if (IsLocal) netUpdate = true;
  }

  public override void UpdateUsing() {
    player.controlJump = false;
  }

  public override void UpdateCooldown() {
    if (abilities.burrow) return;
    base.UpdateCooldown();
  }

  public override void PreUpdate() {
    if (abilities.burrow) {
      _currentCharge = 0;
      _currentGrace = 0;
      return;
    }

    if (!Charged && CanCharge) {
      if (_currentCharge == 0) {
        PlaySound("Ori/ChargeJump/seinChargeJumpChargeB", 0.6f, .2f);
      }

      _currentCharge++;
      if (_currentCharge > MaxCharge) {
        _currentCharge = MaxCharge;
        PlaySound("Ori/ChargeJump/seinChargeJumpChargeB", 0.6f, .2f);
      }
    } else {
      if (!CanCharge && !Grace) _currentCharge--;
      if (_currentCharge < 0) _currentCharge = 0;
    }

    if (CanUse && Grace && input.charge.Current && input.jump.JustPressed) {
      StartChargeJump();
      SetState(AbilityState.Active);
    }
    else if (Charged) {
      UpdateCharged();
      if (IsGrounded && CanCharge) {
        _currentGrace = MaxGrace;
      }
      else {
        _currentGrace--;
        if (_currentGrace == 0) {
          PlaySound("Ori/ChargeDash/seinChargeDashUncharge", 0.6f, .3f);
        }
      }
    }

    if (Active && stateTime >= Duration) {
      SetState(AbilityState.Inactive);
    }
  }

  public override void ReadPacket(BinaryReader r) {
    player.position = r.ReadVector2();
    player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.WriteVector2(player.position);
    packet.WriteVector2(player.velocity);
  }
}
