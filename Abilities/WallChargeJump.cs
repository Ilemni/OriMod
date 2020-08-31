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
    public override byte Level => abilities.climb.Unlocked ? abilities.chargeJump.Level : (byte)0;

    internal override bool CanUse => base.CanUse && Charged && CanCharge;
    protected override int Cooldown => (int)(Config.WCJumpCooldown * 30);
    protected override Color RefreshColor => Color.Blue;

    private static int MaxCharge => 35;
    private static int Duration => Speeds.Length - 1;
    private static float[] Speeds => _speeds ?? (_speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    });
    private static float[] _speeds;
    private static float SpeedMultiplier => Config.WCJumpSpeedMultipler * 0.5f;
    private static float MaxAngle => Config.WCJumpMaxAngle;

    internal bool CanCharge => base.CanUse && abilities.climb.IsCharging;
    internal bool Charged => currentCharge >= MaxCharge;
    private int currentCharge;
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

    public Projectile PlayerHitboxProjectile { get; private set; }

    private readonly RandomChar randChar = new RandomChar();

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

    private void Start() {
      oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + randChar.NextNoRepeat(3));
      currentCharge = 0;
      PlayerHitboxProjectile = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, player.whoAmI, 0, 1)];
      PutOnCooldown();
      // TODO: multiplayer sync of direction
      // Currently it is very, very incorrect to use mouse position for multiplayer clients
      player.velocity = direction * Speeds[0] * SpeedMultiplier;
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
      float speed = Speeds[CurrentTime] * SpeedMultiplier;
      player.velocity = direction * speed;
      player.direction = Math.Sign(player.velocity.X);
      player.maxFallSpeed = Math.Abs(player.velocity.Y);
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
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f, localOnly: true);
        }
        currentCharge++;
        if (currentCharge > MaxCharge) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f, localOnly: true);
        }
      }
      if (CanUse && oPlayer.justPressedJumped) {
        Start();
        SetState(State.Active);
      }
      else if (Charged) {
        UpdateCharged();
        if (IsLocal) {
          direction = GetMouseDirection(out float angle);
          Angle = angle;
        }
        if (!CanCharge) {
          currentCharge = 0;
          oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f, localOnly: true);
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
