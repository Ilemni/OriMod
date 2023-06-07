using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a charged jump off walls.
  /// </summary>
  public sealed class WallChargeJump : Ability<OriAbilityManager> {
    static WallChargeJump() => OriMod.OnUnload += Unload;

    public override int Id => AbilityId.WallChargeJump;
    public override bool Unlocked => abilities.climb.Unlocked && levelableDependency.Level >= 2;
    public override ILevelable levelableDependency => abilities.chargeJump;

    public override bool CanUse => base.CanUse && Charged && CanCharge;

    private static int MaxCharge => 35;
    private static int Duration => Speeds.Length - 1;

    private static float[] Speeds => _speeds ?? (_speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f,
      12f
    });

    private static float[] _speeds;
    private static float MaxAngle => 0.65f;

    public bool CanCharge => base.CanUse && abilities.climb.IsCharging;
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
        netUpdate = true;
      }
    }

    private float _angle;
    private Vector2 _direction;
    public float XDirection => _direction.X < 0 ? -1 : 1;

    private readonly RandomChar _randChar = new RandomChar();

    private void Start() {
      abilities.oPlayer.PlaySound("Ori/ChargeJump/seinChargeJumpJump" + _randChar.NextNoRepeat(3), 0.8f);
      _currentCharge = 0;
      Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f,
        player.whoAmI, 0, 1);
      StartCooldown();
      // TODO: multiplayer sync of direction
      // Currently it is very, very incorrect to use mouse position for multiplayer clients
      player.velocity = _direction * Speeds[0] * 0.5f;
    }

    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust.NewDust(player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.Blue);
      }
    }

    public override void ReadPacket(BinaryReader r) {
      _currentCharge = r.ReadInt32();
      _direction = r.ReadVector2();
      Angle = r.ReadSingle();
      player.position = r.ReadVector2();
      player.velocity = r.ReadVector2();
    }

    public override void WritePacket(ModPacket packet) {
      packet.Write(_currentCharge);
      packet.WriteVector2(_direction);
      packet.Write(Angle);
      packet.WriteVector2(player.position);
      packet.WriteVector2(player.velocity);
    }

    public override void UpdateActive() {
      float speed = Speeds[stateTime] * 0.5f;
      player.velocity = _direction * speed;
      player.direction = Math.Sign(player.velocity.X);
      player.maxFallSpeed = Math.Abs(player.velocity.Y);
      player.controlJump = false;
      player.controlLeft = false;
      player.controlRight = false;

      if (IsLocal) netUpdate = true;
    }

    public override void PreUpdate() {
      if (abilities.burrow) {
        _currentCharge = 0;
        return;
      }

      if (InUse) UpdateCooldown();
      if (IsLocal && !Charged && CanCharge) {
        if (_currentCharge == 0) {
          abilities.oPlayer.PlayLocalSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }

        _currentCharge++;
        netUpdate = true;
        if (_currentCharge > MaxCharge) {
          abilities.oPlayer.PlayLocalSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
      }

      if (CanUse && abilities.oPlayer.input.jump.JustPressed) {
        Start();
        SetState(AbilityState.Active);
      }
      else if (Charged) {
        UpdateCharged();
        if (IsLocal) {
          _direction = OriUtils.GetMouseDirection(abilities.oPlayer, out float angle,
            new Vector2(-abilities.climb.wallDirection, player.gravDir), MaxAngle);
          Angle = angle;
          if (!CanCharge) {
            _currentCharge = 0;
            abilities.oPlayer.PlayLocalSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
          }
        }
      }

      if (!Active || stateTime <= Duration) return;
      SetState(AbilityState.Inactive);
      netUpdate = false; // Deterministic
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}