using System;
using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a charged jump off walls.
  /// </summary>
  public sealed class WallChargeJump : Ability {
    static WallChargeJump() => OriMod.OnUnload += Unload;
    internal WallChargeJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.WallChargeJump;
    public override byte Level => (byte)(abilities.climb.Unlocked && levelableDependency.Level >= 2 ? 1 : 0);
    public override ILevelable levelableDependency => abilities.chargeJump;

    internal override bool CanUse => base.CanUse && Charged && CanCharge;

    private static int MaxCharge => 35;
    private static int Duration => Speeds.Length - 1;
    private static float[] Speeds => _speeds ?? (_speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    });
    private static float[] _speeds;
    private static float MaxAngle => 0.65f;

    public bool CanCharge => base.CanUse && abilities.climb.IsCharging;
    public bool Charged => currentCharge >= MaxCharge;
    private int currentCharge;
    /// <summary>
    /// Angle that the player is facing.
    /// </summary>
    public float Angle {
      get => _angle;
      set {
        if (value != _angle) {
          _angle = value;
          netUpdate = true;
        }
      }
    }
    private float _angle;
    private Vector2 direction;
    public float xDirection => direction.X < 0 ? -1 : 1;

    private readonly RandomChar randChar = new RandomChar();

    private void Start() {
      oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + randChar.NextNoRepeat(3), 0.8f);
      currentCharge = 0;
      Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, player.whoAmI, 0, 1);
      PutOnCooldown();
      // TODO: multiplayer sync of direction
      // Currently it is very, very incorrect to use mouse position for multiplayer clients
      player.velocity = direction * Speeds[0] * 0.5f;
    }

    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust.NewDust(player.Center, 12, 12, ModContent.DustType<Dusts.AbilityRefreshedDust>(), newColor: Color.Blue);
      }
    }

    protected override void ReadPacket(BinaryReader r) {
      currentCharge = r.ReadInt32();
      direction = r.ReadVector2();
      Angle = r.ReadSingle();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(currentCharge);
      packet.WriteVector2(direction);
      packet.Write(Angle);
    }

    protected override void UpdateActive() {
      float speed = Speeds[CurrentTime] * 0.5f;
      player.velocity = direction * speed;
      player.direction = Math.Sign(player.velocity.X);
      player.maxFallSpeed = Math.Abs(player.velocity.Y);
      player.controlJump = false;
      player.controlLeft = false;
      player.controlRight = false;
    }

    internal override void Tick() {
      if (abilities.burrow) {
        currentCharge = 0;
        return;
      }
      TickCooldown();
      if (IsLocal && !Charged && CanCharge) {
        if (currentCharge == 0) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f, localOnly: true);
        }
        currentCharge++;
        netUpdate = true;
        if (currentCharge > MaxCharge) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f, localOnly: true);
        }
      }
      if (CanUse && input.jump.JustPressed) {
        Start();
        SetState(State.Active);
      }
      else if (Charged) {
        UpdateCharged();
        if (IsLocal) {
          direction = OriUtils.GetMouseDirection(oPlayer, out float angle, new Vector2(-abilities.climb.wallDirection, player.gravDir), MaxAngle);
          Angle = angle;
          if (!CanCharge) {
            currentCharge = 0;
            oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f, localOnly: true);
          }
        }
      }
      if (Active) {
        if (CurrentTime > Duration) {
          SetState(State.Inactive);
          netUpdate = false; // Deterministic
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
