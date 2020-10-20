using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a quick and high jump that can deal damage to enemies.
  /// </summary>
  public sealed class ChargeJump : Ability, ILevelable {
    static ChargeJump() => OriMod.OnUnload += Unload;
    internal ChargeJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.ChargeJump;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 5;

    internal override bool CanUse => base.CanUse && !InUse && Charged &&
      !abilities.burrow && !abilities.chargeDash && !abilities.climb && !abilities.dash && !abilities.launch &&
      !abilities.stomp && !abilities.wallChargeJump;
    
    protected override int Cooldown => 120;
    protected override Color RefreshColor => Color.Blue;

    internal bool CanCharge => base.CanUse && !InUse && oPlayer.IsGrounded && input.charge.Current;
    private bool Charged => currentCharge >= MaxCharge;

    /// <summary>
    /// Coyote jump, how long to allow CJumping when no longer valid to do so.
    /// </summary>
    private static int MaxGrace => 25;
    private static int MaxCharge => 35;

    private static float[] Speeds => _speeds ?? (_speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    });
    private static float[] _speeds;
    private static int Duration => Speeds.Length;

    private int currentCharge;
    private int currentGrace;

    /// <summary>
    /// Projectile created while Charge Jumping to damage enemies.
    /// </summary>
    /// <remarks>
    /// This damage aspect is derived from the Ori games. May be unbalanced or unnecessary here.
    /// </remarks>
    public Projectile PlayerHitboxProjectile { get; private set; }

    private readonly RandomChar rand = new RandomChar();

    private void StartChargeJump() {
      oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + rand.NextNoRepeat(3));
      currentCharge = 0;
      PlayerHitboxProjectile = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, player.whoAmI, 0, 1);
      PutOnCooldown();
      abilities.climb.SetState(State.Inactive);
    }

    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust.NewDust(player.Center, 12, 12, ModContent.DustType<Dusts.AbilityRefreshedDust>(), newColor: Color.Blue);
      }
    }

    protected override void UpdateActive() {
      float speed = Speeds[CurrentTime] * 0.35f;
      player.velocity.Y = speed * -player.gravDir;
      oPlayer.immuneTimer = 12;
    }

    protected override void UpdateUsing() {
      player.controlJump = false;
    }

    internal override void Tick() {
      if (abilities.burrow) {
        currentCharge = 0;
        currentGrace = 0;
        return;
      }
      if (!Refreshed && !InUse) {
        currentCooldown--;
        if (currentCooldown < 0) {
          Refreshed = true;
        }
      }
      if (!Charged && CanCharge) {
        if (currentCharge == 0) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 0.6f, .2f);
        }
        currentCharge++;
        if (currentCharge > MaxCharge) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 0.6f, .2f);
        }
      }
      if (CanUse && input.jump.JustPressed) {
        StartChargeJump();
        SetState(State.Active);
      }
      else if (Charged) {
        UpdateCharged();
        if (CanCharge) {
          currentGrace = MaxGrace;
        }
        else {
          currentGrace--;
          if (currentGrace < 0) {
            currentCharge = 0;
            oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 0.6f, .3f);
          }
        }
      }
      if (Active) {
        if (CurrentTime >= Duration) {
          SetState(State.Inactive);
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
