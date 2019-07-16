using Terraria;

namespace OriMod.Projectiles.Abilities {
  internal class StompEnd : AbilityProjectile {
    internal override int abilityID => AbilityID.Stomp;
    
    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 600;
      projectile.height = 320;
      projectile.penetrate = 8;
    }
    public override bool PreAI() => false;

    private void ModifyHitAny(Entity entity, ref int damage, ref bool crit) {
      if (Main.rand.Next(5) == 1) {
        crit = true;
      }
      int oldDamage = damage;
      damage = (int)((float)damage * projectile.penetrate / projectile.maxPenetrate);
    }
    public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => ModifyHitAny(target, ref damage, ref crit);
    public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) => ModifyHitAny(target, ref damage, ref crit);
    public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) => ModifyHitAny(target, ref damage, ref crit);
  }
}