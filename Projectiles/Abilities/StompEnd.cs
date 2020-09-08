using OriMod.Abilities;
using OriMod.Utilities;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for the impact of a <see cref="Stomp"/>. Deals damage to NPCs.
  /// <para>As the number of targets to hit grows, the damage dealt to the next target is reduced.</para>
  /// </summary>
  public sealed class StompEnd : AbilityProjectile {
    public override byte abilityID => AbilityID.Stomp;

    public float Knockback {
      get {
        switch (Level) {
          case 1: return 16;
          case 2: return 30;
          default: return 10 + Level * 12;
        }
      }
    }

    public int MaxPenetrate {
      get {
        switch (Level) {
          case 1: return 8;
          case 2: return 20;
          default: return Level * 10;
        }
      }
    }

    public int Width {
      get {
        switch (Level) {
          case 1: return 600;
          case 2: return 660;
          default: return 400 + Level * 100;
        }
      }
    }

    public int Height {
      get {
        switch (Level) {
          case 1: return 320;
          case 2: return 360;
          default: return 240 + Level * 60;
        }
      }
    }

    public override void SetDefaults() {
      base.SetDefaults();
      projectile.width = 600;
      projectile.height = 320;
    }

    public override bool PreAI() {
      // SetDefaults called before Projectile.NewProjectile(...) sets ai fields, so we need a later hook
      if (projectile.maxPenetrate != MaxPenetrate) {
        projectile.penetrate = projectile.maxPenetrate = MaxPenetrate;
        projectile.width = Width;
        projectile.height = Height;
      }
      return false;
    }

    private void ModifyHitAny(Entity target, ref int damage, ref bool crit) {
      if (!crit && Main.rand.Next(5) == 1) {
        crit = true;
      }
      var multiplier = projectile.penetrate / projectile.maxPenetrate;
      damage = (int)(damage * 0.6f + (damage * 0.4f * multiplier));
      var vect = target.Center - oPlayer.player.Center;
      var dist = target.Distance(oPlayer.player.Center);
      var kb = Knockback * (160 - dist) / 160;
      if (kb < 6) {
        kb = 6;
      }
      target.velocity += vect.Normalized() * kb;
    }

    public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => ModifyHitAny(target, ref damage, ref crit);
    public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) => ModifyHitAny(target, ref damage, ref crit);
    public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) => ModifyHitAny(target, ref damage, ref crit);
  }
}
