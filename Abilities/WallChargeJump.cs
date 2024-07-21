using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a charged jump off walls.
/// </summary>
public sealed class WallChargeJump : OriAbility {
  public override int Id => AbilityId.WallChargeJump;
  public override bool Unlocked => Abilities.Climb.Unlocked && LevelableDependency.Level >= 2;
  public override ILevelable LevelableDependency => Abilities.ChargeJump;

  public override bool CanUse => base.CanUse && Charged && CanCharge;

  private static int MaxCharge => 35;
  private static int Duration => Speeds.Length - 1;

  private static float[] Speeds => _speeds ??= Unloadable.New(new float[20] {
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f,
    12f
  }, () => _speeds = null);

  private static float[] _speeds;
  private static float MaxAngle => 0.65f;

  public bool CanCharge => base.CanUse && Abilities.Climb.IsCharging && !Player.shimmering;
  public bool Charged => _currentCharge >= MaxCharge;
  private int _currentCharge;

  /// <summary>
  /// Angle that the player is facing.
  /// </summary>
  public float Angle {
    get => _angle;
    set {
      if (Math.Abs(value - _angle) < 0.01f) return;
      _angle = value;
      NetUpdate = true;
    }
  }
  
  private float _angle;
  private Vector2 _direction;
  public float XDirection => _direction.X < 0 ? -1 : 1;

  private readonly RandomChar _randChar = new();

  private void Start() {
    PlaySound("Ori/ChargeJump/seinChargeJumpJump" + _randChar.NextNoRepeat(3), 0.8f);
    _currentCharge = 0;
    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f,
      Player.whoAmI, 0, 1);
    StartCooldown();
    // TODO: multiplayer sync of direction
    // Currently it is very, very incorrect to use mouse position for multiplayer clients
    Player.velocity = _direction * Speeds[0] * 0.5f;
  }

  private void UpdateCharged() {
    if (Main.rand.NextFloat() < 0.7f) {
      Dust.NewDust(Player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
    }
  }

  public override void ReadPacket(BinaryReader r) {
    _currentCharge = r.ReadInt32();
    _direction = r.ReadVector2();
    Angle = r.ReadSingle();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_currentCharge);
    packet.WriteVector2(_direction);
    packet.Write(Angle);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }

  public override void UpdateActive() {
    float speed = Speeds[StateTime] * 0.5f;
    Player.velocity = _direction * speed;
    Player.direction = Math.Sign(Player.velocity.X);
    Player.maxFallSpeed = Math.Abs(Player.velocity.Y);
    Player.controlJump = false;
    Player.controlLeft = false;
    Player.controlRight = false;

    if (IsLocal) NetUpdate = true;
  }

  public override void PreUpdate() {
    if (Abilities.Burrow) {
      _currentCharge = 0;
      return;
    }

    if (InUse) UpdateCooldown();
    if (IsLocal && !Charged && CanCharge) {
      if (_currentCharge == 0) {
        PlayLocalSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
      }

      _currentCharge++;
      NetUpdate = true;
      if (_currentCharge > MaxCharge) {
        PlayLocalSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
      }
    }

    if (CanUse && Input.Jump.JustPressed) {
      Start();
      SetState(AbilityState.Active);
    }
    else if (Charged) {
      UpdateCharged();
      if (IsLocal) {
        _direction = OriUtils.GetMouseDirection(oPlayer, out float angle,
          new Vector2(-Abilities.Climb.WallDirection, Player.gravDir), MaxAngle);
        Angle = angle;
        if (!CanCharge) {
          _currentCharge = 0;
          PlayLocalSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
        }
      }
    }

    if (!Active || StateTime <= Duration) return;
    SetState(AbilityState.Inactive);
    NetUpdate = false; // Deterministic
  }
}
