using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities;

/// <summary>
/// Projectile hitbox for the impact of a <see cref="Stomp"/>. Deals damage to NPCs.
/// <para>As the number of targets to hit grows, the damage dealt to the next target is reduced.</para>
/// </summary>
public sealed class StompEnd : OriAbilityProjectile<Stomp> {
  private float Knockback =>
    Level switch {
      1 => 16,
      2 => 30,
      _ => 10 + Level * 12
    };

  private int MaxPenetrate =>
    Level switch {
      1 => 8,
      2 => 20,
      _ => Level * 10
    };

  private int Width =>
    Level switch {
      1 => 600,
      2 => 660,
      _ => 400 + Level * 100
    };

  private int Height =>
    Level switch {
      1 => 320,
      2 => 360,
      _ => 240 + Level * 60
    };

  public override void SetDefaults() {
    base.SetDefaults();
    Projectile.width = 600;
    Projectile.height = 320;
    Projectile.CritChance = 20;
  }

  public override bool PreAI() {
    // SetDefaults called before Projectile.NewProjectile(...) sets ai fields, so we need a later hook
    if (Projectile.maxPenetrate == MaxPenetrate) return false;

    Projectile.penetrate = Projectile.maxPenetrate = MaxPenetrate;
    Projectile.width = Width;
    Projectile.height = Height;
    return false;
  }

  private void ModifyHitAny(Entity target) {
    Vector2 playerCenter = Player.Center;
    Vector2 direction = (target.Center - playerCenter).SafeNormalize(default);

    float kb = Math.Max(6, Knockback * (160 - target.Distance(playerCenter)) / 160) * target switch {
      NPC npc => npc.knockBackResist,
      Player player => -player.noKnockback.ToInt(),
      _ => 1
    };

    target.velocity += direction * kb;
  }

  public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
    // Damage from 100% to 60% as penetrate decreases
    float percentPenetrateLeft = (float)Projectile.penetrate / Projectile.maxPenetrate;
    modifiers.FinalDamage.Scale(0.6f + 0.4f * percentPenetrateLeft);
    ModifyHitAny(target);
  }

  public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
    // Damage from 100% to 60% as penetrate decreases
    float percentPenetrateLeft = (float)Projectile.penetrate / Projectile.maxPenetrate;
    modifiers.FinalDamage.Scale(0.6f + 0.4f * percentPenetrateLeft);
    if (!target.immortal) {
      // Don't knockback target dummies
      ModifyHitAny(target);
    }
  }
}
