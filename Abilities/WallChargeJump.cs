using System;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a charged jump off walls.
  /// </summary>
  public sealed class WallChargeJump : Ability {
    static WallChargeJump() => OriMod.OnUnload += Unload;
    internal WallChargeJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.WallChargeJump;

    internal override bool CanUse => base.CanUse && Charged && CanCharge;
    protected override int Cooldown => (int)(Config.WCJumpCooldown * 30);
    protected override Color RefreshColor => Color.Blue;

    internal bool CanCharge => base.CanUse && abilities.climb.IsCharging;
    private int MaxCharge => 35;
    private int Duration => Speeds.Length - 1;
    private static float[] Speeds => _speeds ?? (_speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    });
    private static float[] _speeds;
    private float SpeedMultiplier => Config.WCJumpSpeedMultipler * 0.5f;
    private float MaxAngle => Config.WCJumpMaxAngle;

    internal bool Charged => currentCharge >= MaxCharge;
    private int currentCharge;
    private Vector2 Direction;

    public Projectile PlayerHitboxProjectile { get; private set; }

    private readonly RandomChar randChar = new RandomChar();

    internal Vector2 GetMouseDirection() => GetMouseDirection(out float _);
    internal Vector2 GetMouseDirection(out float angle) {
      Vector2 mouse = Main.MouseWorld - player.Center;
      mouse.X *= -abilities.climb.wallDirection;
      mouse.Y *= player.gravDir;
      mouse += player.Center;
      angle = Utils.Clamp(player.AngleTo(mouse), -MaxAngle, MaxAngle);
      Vector2 dir = Vector2.UnitX.RotatedBy(angle);
      dir.X *= -abilities.climb.wallDirection;
      dir.Y *= player.gravDir;
      return dir;
    }

    private void StartWallChargeJump() {
      oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + randChar.NextNoRepeat(3));
      currentCharge = 0;
      PlayerHitboxProjectile = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, player.whoAmI, 0, 1)];
      PutOnCooldown();
      Direction = GetMouseDirection();
      player.velocity = Direction * Speeds[0] * SpeedMultiplier;
    }

    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust.NewDust(player.Center, 12, 12, ModContent.DustType<Dusts.AbilityRefreshedDust>(), newColor: Color.Blue);
      }
    }

    protected override void UpdateActive() {
      float speed = Speeds[CurrentTime] * SpeedMultiplier;
      player.velocity = Direction * speed;
      player.direction = Math.Sign(player.velocity.X);
      player.maxFallSpeed = Math.Abs(player.velocity.Y);
    }

    protected override void UpdateUsing() {
      player.controlJump = false;
    }

    internal override void Tick() {
      if (abilities.burrow.InUse) {
        currentCharge = 0;
        return;
      }
      TickCooldown();
      if (!Charged && CanCharge) {
        if (currentCharge == 0) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
        currentCharge++;
        if (currentCharge > MaxCharge) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
      }
      if (CanUse && IsLocal && PlayerInput.Triggers.JustPressed.Jump) {
        StartWallChargeJump();
        SetState(State.Active);
      }
      else if (Charged) {
        UpdateCharged();
        if (!CanCharge) {
          currentCharge = 0;
          oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
        }
      }
      if (Active) {
        if (CurrentTime > Duration) {
          SetState(State.Inactive);
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
