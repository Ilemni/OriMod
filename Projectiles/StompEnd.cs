using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles {
  public class StompEnd : ModProjectile {
    private OriPlayer _owner;
    public OriPlayer Owner => _owner ?? (_owner = Main.player[projectile.owner].GetModPlayer<OriPlayer>());
    private Abilities.Stomp stomp => Owner.stomp;
    public override string Texture => "OriMod/Projectiles/StompHitbox";
    public override void SetStaticDefaults() { }

    public override void SetDefaults() {
      projectile.width = 600;
      projectile.height = 320;
      projectile.timeLeft = 2;
      projectile.penetrate = 999;
      projectile.magic = true;
      projectile.tileCollide = false;
      projectile.ignoreWater = true;
      projectile.friendly = true;
    }
    public override bool PreAI() => false;
    public override bool ShouldUpdatePosition() => false;

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      if (Main.rand.Next(5) == 1) {
        crit = true;
      }
    }
  }
}