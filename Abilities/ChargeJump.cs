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
    !Abilities.Burrow && !Abilities.Climb && !Abilities.Launch &&
    !Abilities.Stomp && !Abilities.WallChargeJump;

  public override int Cooldown => 120;
  public override void OnRefreshed() => Abilities.RefreshParticles(Color.Blue);

  private bool CanCharge => base.CanUse && Input.Charge.Current;
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
    Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
      ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, Player.whoAmI, 0, 1);
    StartCooldown();
    Abilities.Climb.SetState(AbilityState.Inactive);
  }

  private void UpdateCharged() {
    if (Main.rand.NextFloat() < 0.7f) {
      Dust.NewDust(Player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
    }
  }

  public override void UpdateActive() {
    float speed = Speeds[StateTime] * 0.35f;
    Player.velocity.Y = speed * -Player.gravDir;
    oPlayer.ImmuneTimer = 12;

    if (IsLocal) NetUpdate = true;
  }

  public override void UpdateUsing() {
    Player.controlJump = false;
  }

  public override void UpdateCooldown() {
    if (Abilities.Burrow) return;
    base.UpdateCooldown();
  }

  public override void PreUpdate() {
    if (Abilities.Burrow) {
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

    if (CanUse && Grace && Input.Charge.Current && Input.Jump.JustPressed) {
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

    if (Active && StateTime >= Duration) {
      SetState(AbilityState.Inactive);
    }
  }

  public override void ReadPacket(BinaryReader r) {
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }
}
