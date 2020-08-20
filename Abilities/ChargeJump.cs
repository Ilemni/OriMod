using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a large jump that can deal damage to enemies.
  /// </summary>
  public sealed class ChargeJump : Ability {
    internal ChargeJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.ChargeJump;

    internal override bool UpdateCondition => InUse || oPlayer.Input(OriMod.ChargeKey.Current);
    internal override bool CanUse => base.CanUse && !InUse && Charged && !Manager.burrow.InUse && !Manager.climb.InUse;
    protected override int Cooldown => (int)(Config.CJumpCooldown * 30);
    protected override Color RefreshColor => Color.Blue;

    internal bool CanCharge => base.CanUse && !InUse && oPlayer.IsGrounded && oPlayer.Input(OriMod.ChargeKey.Current);
    private int MaxCharge => 35;
    private bool Charged => currentCharge >= MaxCharge;

    /// <summary>
    /// Coyote jump
    /// </summary>
    private int ChargeGrace => 25;

    private static readonly float[] Speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    };
    private float SpeedMultiplier => Config.CJumpSpeedMultiplier * 0.35f;
    private int Duration => Speeds.Length;

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
      PlayerHitboxProjectile = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f, player.whoAmI, 0, 1)];
      PutOnCooldown();
      Manager.climb.SetState(State.Inactive);
    }

    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust.NewDust(player.Center, 12, 12, ModContent.DustType<Dusts.AbilityRefreshedDust>(), newColor: Color.Blue);
      }
    }

    protected override void UpdateActive() {
      float speed = Speeds[CurrentTime - 1] * SpeedMultiplier;
      player.velocity.Y = speed * -player.gravDir;
      oPlayer.ImmuneTimer = 12;
    }

    protected override void UpdateUsing() {
      player.controlJump = false;
    }

    internal override void Tick() {
      if (Manager.burrow.InUse) {
        currentCharge = 0;
        currentGrace = 0;
        return;
      }
      if (!Refreshed && !InUse) {
        CurrentCooldown--;
        if (CurrentCooldown < 0) {
          Refreshed = true;
        }
      }
      if (!Charged && CanCharge) {
        if (currentCharge == 0) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
        currentCharge++;
        if (currentCharge > MaxCharge) {
          oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
      }
      if (CanUse && oPlayer.justJumped) {
        StartChargeJump();
        SetState(State.Active);
      }
      else if (Charged) {
        UpdateCharged();
        if (CanCharge) {
          currentGrace = ChargeGrace;
        }
        else {
          currentGrace--;
          if (currentGrace < 0) {
            currentCharge = 0;
            oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
          }
        }
      }
      if (Active) {
        CurrentTime++;
        if (CurrentTime > Duration) {
          if (oPlayer.Input(PlayerInput.Triggers.Current.Jump)) {
            SetState(State.Ending);
          }
          else {
            SetState(State.Inactive);
          }
          CurrentTime = 0;
        }
      }
      if (Ending && !oPlayer.Input(PlayerInput.Triggers.Current.Jump)) {
        SetState(State.Inactive);
      }
    }
  }
}
