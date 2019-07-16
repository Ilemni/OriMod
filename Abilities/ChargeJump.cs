using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Abilities {
  public class ChargeJump : Ability {
    internal ChargeJump(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    public override int id => AbilityID.ChargeJump;
    internal override bool DoUpdate => InUse || oPlayer.Input(OriMod.ChargeKey.Current);
    internal override bool CanUse => base.CanUse && !InUse && Charged && !Handler.burrow.InUse && !Handler.climb.InUse;
    protected override int Cooldown => (int)(Config.CJumpCooldown * 30);
    protected override Color RefreshColor => Color.Blue;
    
    internal bool CanCharge => base.CanUse && !InUse && oPlayer.IsGrounded && (Handler.lookUp.InUse || oPlayer.Input(OriMod.ChargeKey.Current));
    private int MaxCharge => 35;
    private int ChargeGrace => 15;
    private static readonly float[] Speeds = new float[20] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f, 12f
    };
    private float SpeedMultiplier => Config.CJumpSpeedMultiplier * 0.35f;
    private int Duration => Speeds.Length;
    
    private bool Charged;
    private int CurrCharge;
    private int CurrGrace;
    
    public Projectile Proj { get; private set; }
    
    private void StartChargeJump() {
      oPlayer.PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + OriPlayer.RandomChar(3, ref currRand));
      Charged = false;
      CurrCharge = 0;
      Proj = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("ChargeJumpProjectile"), 30, 0f, player.whoAmI, 0, 1)];
      PutOnCooldown();
      Handler.climb.Inactive = true;
    }
    private void UpdateCharged() {
      if (Main.rand.NextFloat() < 0.7f) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("AbilityRefreshedDust"), newColor:Color.Blue)];
      }
    }
    
    protected override void UpdateActive() {
      float speed = Speeds[CurrTime - 1] * SpeedMultiplier;
      player.velocity.Y = speed * -player.gravDir;
      oPlayer.ImmuneTimer = 12;
    }
    protected override void UpdateUsing() {
      player.controlJump = false;
    }
    
    internal override void Tick() {
      if (Handler.burrow.InUse) {
        Charged = false;
        CurrCharge = 0;
        CurrGrace = 0;
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
      if (CanUse && oPlayer.justJumped) {
        StartChargeJump();
        Active = true;
      }
      else if (Charged) {
        UpdateCharged();
        if (CanCharge) {
          CurrGrace = ChargeGrace;
        }
        else {
          CurrGrace--;
          if (CurrGrace < 0) {
            Charged = false;
            CurrCharge = 0;
            oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
          }
        }
      }
      if (Active) {
        CurrTime++;
        if (CurrTime > Duration) {
          if (oPlayer.Input(OriPlayer.Current.Jump)) {
            Ending = true;
          }
          else {
            Inactive = true;
          }
          CurrTime = 0;
        }
      }
      if (Ending && !oPlayer.Input(OriPlayer.Current.Jump)) {
        Inactive = true;
      }
    }
  }
}