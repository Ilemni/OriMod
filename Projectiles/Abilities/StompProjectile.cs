using System;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Projectiles.Abilities; 

/// <summary>
/// Projectile hitbox for when the player is in the stomping animation. Used to end <see cref="Stomp"/>.
/// <para>Ending stomp spawns a <see cref="StompEnd"/> projectile to deal damage.</para>
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class StompProjectile : OriAbilityProjectile {
  public override int Id => AbilityId.Stomp;

  public override void SetDefaults() {
    base.SetDefaults();
    Projectile.width = 40;
    Projectile.height = 56;
  }

  protected override void Behavior() {
    base.Behavior();
    // Height is stretched based on velocity.
    Projectile.height = Math.Max(56, (int)(aPlayer.Player.velocity.Y * 2));
    Projectile.position.Y += 10;
  }

  public override void OnHitNPC(NPC target, NPC.HitInfo modifiers, int damageDone) {
    Stomp stomp = abilities.stomp;
    if (target.life > 0 && stomp.InUse) {
      stomp.EndStomp();
    }
  }
}
