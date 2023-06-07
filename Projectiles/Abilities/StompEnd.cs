using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Utilities;
using Terraria;

namespace OriMod.Projectiles.Abilities;

/// <summary>
/// Projectile hitbox for the impact of a <see cref="Stomp"/>. Deals damage to NPCs.
/// <para>As the number of targets to hit grows, the damage dealt to the next target is reduced.</para>
/// </summary>
public sealed class StompEnd : OriAbilityProjectile {
  public override int Id => AbilityId.Stomp;

  private float Knockback =>
    level switch {
      1 => 16,
      2 => 30,
      _ => 10 + level * 12
    };

  private int MaxPenetrate =>
    level switch {
      1 => 8,
      2 => 20,
      _ => level * 10
    };

  private int Width =>
    level switch {
      1 => 600,
      2 => 660,
      _ => 400 + level * 100
    };

  private int Height =>
    level switch {
      1 => 320,
      2 => 360,
      _ => 240 + level * 60
    };

  public override void SetDefaults() {
    base.SetDefaults();
    Projectile.width = 600;
    Projectile.height = 320;
  }

  public override bool PreAI() {
    // SetDefaults called before Projectile.NewProjectile(...) sets ai fields, so we need a later hook
    if (Projectile.maxPenetrate == MaxPenetrate) return false;
    Projectile.penetrate = Projectile.maxPenetrate = MaxPenetrate;
    Projectile.width = Width;
    Projectile.height = Height;
    return false;
  }

  private void ModifyHitAny(Entity target, ref int damage, ref bool crit) {
    if (!crit && Main.rand.NextBool(5)) {
      crit = true;
    }
    int multiplier = Projectile.penetrate / Projectile.maxPenetrate;
    damage = (int)(damage * 0.6f + damage * 0.4f * multiplier);
    Vector2 vector = target.Center - aPlayer.Player.Center;
    float dist = target.Distance(aPlayer.Player.Center);
    float kb = Knockback * (160 - dist) / 160;
    if (kb < 6) {
      kb = 6;
    }
    target.velocity += vector.Normalized() * kb;
  }

  public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) => ModifyHitAny(target, ref damage, ref crit);
  public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) => ModifyHitAny(target, ref damage, ref crit);
  public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) => ModifyHitAny(target, ref damage, ref crit);
}
