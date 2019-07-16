using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Abilities {
  internal abstract class AbilityProjectile : ModProjectile {
    internal virtual int abilityID => -1;
    private OriPlayer _oPlayer;
    internal OriPlayer oPlayer => _oPlayer ?? (_oPlayer = Main.player[projectile.owner].GetModPlayer<OriPlayer>());
    internal Ability ability => oPlayer.Abilities[abilityID];

    public override string Texture => "OriMod/Projectiles/Abilities/Blank";

    public override void SetDefaults() {
      projectile.timeLeft = 2;
      projectile.penetrate = 999;
      projectile.magic = true;
      projectile.tileCollide = false;
      projectile.ignoreWater = true;
      projectile.friendly = true;
    }
    
    public override bool ShouldUpdatePosition() => false;
    public override void AI() {
      CheckAbilityActive();
      Behavior();
    }
    /// <summary>
    /// Used to determine timeLeft or active state of the projectile.
    /// 
    /// Defaults to keeping timeLeft above 0 if ability is in use.
    /// </summary>
    internal virtual void CheckAbilityActive() {
      if (ability.InUse) {
        projectile.timeLeft = 2;
      }
    }
    /// <summary>
    /// How the projectile behaves.
    /// 
    /// Defaults to setting projectile center to player center.
    /// </summary>
    internal virtual void Behavior() {
      projectile.Center = oPlayer.player.Center;
    }
  }
}