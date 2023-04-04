using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Utilities;
using Steamworks;
using Terraria;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Projectile hitbox for the impact of a <see cref="Stomp"/>. Deals damage to NPCs.
  /// <para>As the number of targets to hit grows, the damage dealt to the next target is reduced.</para>
  /// </summary>
    public sealed class StompEnd : OriAbilityProjectile {
    public override int Id => AbilityId.Stomp;

    private float Knockback {
      get {
        switch (level) {
          case 1: return 16;
          case 2: return 30;
          default: return 10 + level * 12;
        }
      }
    }

    private int MaxPenetrate {
      get {
        switch (level) {
          case 1: return 8;
          case 2: return 20;
          default: return level * 10;
        }
      }
    }

    private int Width {
      get {
        switch (level) {
          case 1: return 600;
          case 2: return 660;
          default: return 400 + level * 100;
        }
      }
    }

    private int Height {
      get {
        switch (level) {
          case 1: return 320;
          case 2: return 360;
          default: return 240 + level * 60;
        }
      }
    }

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
      Vector2 vector = target.Center - aPlayer.Player.Center;
      float dist = target.Distance(aPlayer.Player.Center);
      float kb = Knockback * (160.0f - dist) / 160.0f;
      if (kb < 6) {
        kb = 6;
      }
      target.velocity += vector.Normalized() * kb;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
      float multiplier = (float)Projectile.penetrate / Projectile.maxPenetrate;
      modifiers.FinalDamage.Scale(0.6f + 0.4f * multiplier);
      ModifyHitAny(target);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
      float multiplier = (float)Projectile.penetrate / Projectile.maxPenetrate;
      modifiers.FinalDamage.Scale(0.6f + 0.4f * multiplier);
      ModifyHitAny(target);
    }
  }
}
