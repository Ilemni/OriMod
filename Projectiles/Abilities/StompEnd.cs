using Terraria;

namespace OriMod.Projectiles.Abilities {
  public sealed class StompEnd : AbilityProjectile {
    public override byte abilityID => AbilityID.Stomp;

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 600;
      projectile.height = 320;
      projectile.penetrate = OriMod.ConfigAbilities.StompNumTargets;
    }
    public override bool PreAI() => false;

    private void ModifyHitAny(ref int damage, ref bool crit) {
      if (!crit && Main.rand.Next(5) == 1) {
        crit = true;
      }
      damage = (int)((float)damage * projectile.penetrate / projectile.maxPenetrate);
    }

    public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => ModifyHitAny(ref damage, ref crit);
    public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) => ModifyHitAny(ref damage, ref crit);
    public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) => ModifyHitAny(ref damage, ref crit);
  }
}
