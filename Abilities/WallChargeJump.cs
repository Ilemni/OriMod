using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class WallChargeJump : Ability {
    public WallChargeJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }

    internal override bool CanUse => base.CanUse && Charged && CanCharge;
    internal bool CanCharge => base.CanUse && Handler.climb.IsCharging;
    protected override Color RefreshColor => Color.Blue;
    protected override int Cooldown => 420;
    private const int Duration = 20;
    private int CurrTime = 0;
    private static readonly float[] Speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    };
    public Projectile Proj { get; private set; }
    internal bool Charged = false;
    private int MaxCharge => 35;
    private int CurrCharge = 0;
    private Vector2 Direction = Vector2.Zero;
    protected override void UpdateActive() {
      float speed = Speeds[CurrTime - 1] * 0.5f;
      player.velocity = Direction * speed;
      player.direction = Math.Sign(player.velocity.X);
      player.maxFallSpeed = Math.Abs(player.velocity.Y);
    }
    private void StartWallChargeJump() {
      oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + OriPlayer.RandomChar(3, ref currRand));
      Charged = false;
      CurrCharge = 0;
      Proj = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1)];
      if (!Config.BlindForestMovement) {
        Refreshed = false;
        CurrCooldown = Cooldown;
      }
      float angle = Utils.Clamp(player.AngleTo(Main.MouseWorld) * -Handler.climb.WallDir, -0.5f, 0.5f);
      Direction = Vector2.UnitX.RotatedBy(angle);
      Direction *= -Handler.climb.WallDir;
      player.velocity = Direction * Speeds[0] * 0.5f;
    }
    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("AbilityRefreshedDust"), newColor:Color.Blue)];
      }
    }
    internal override void Tick() {
      if (Handler.burrow.InUse) {
        Charged = false;
        CurrCharge = 0;
        return;
      }
      if (!Refreshed && !InUse) {
        CurrCooldown--;
        if (CurrCooldown < 0) {
          Refreshed = true;
        }
      }
      if (!Charged && CanCharge) {
        if (CurrCharge == 0) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
        CurrCharge++;
        if (CurrCharge > MaxCharge) {
          Charged = true;
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
      }
      if (CanUse && oPlayer.Input(PlayerInput.Triggers.JustPressed.Jump)) {
        StartWallChargeJump();
        Active = true;
      }
      else if (Charged) {
        UpdateCharged();
        if (!CanCharge) {
          Charged = false;
          oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
        }
      }
      if (Active) {
        CurrTime++;
        if (CurrTime > Duration) {
          Inactive = true;
          CurrTime = 0;
        }
      }
    }
  }
}