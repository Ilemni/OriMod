using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Base class for ability projectiles. These act more as hitboxes and have no visible texture.
  /// </summary>
  public abstract class AbilityProjectile : ModProjectile {
    /// <summary>
    /// Correlates to a <see cref="AbilityID"/>.
    /// </summary>
    public abstract byte abilityID { get; }

    /// <summary>
    /// THe <see cref="OriPlayer"/> that this <see cref="AbilityProjectile"/> belongs to.
    /// </summary>
    public OriPlayer oPlayer => _oPlayer ?? (_oPlayer = Main.player[projectile.owner].GetModPlayer<OriPlayer>());
    private OriPlayer _oPlayer;

    /// <summary>
    /// The <see cref="Ability"/> that this <see cref="AbilityProjectile"/> belongs to.
    /// </summary>
    public Ability ability => oPlayer.abilities[abilityID];

    public override string Texture => "OriMod/Projectiles/Abilities/Blank";

    public override void SetDefaults() {
      projectile.timeLeft = 2;
      projectile.penetrate = int.MaxValue;
      projectile.magic = true;
      projectile.tileCollide = false;
      projectile.ignoreWater = true;
      projectile.friendly = true;
    }

    public override bool ShouldUpdatePosition() => false;
    public override sealed void AI() {
      CheckAbilityActive();
      Behavior();
    }

    /// <summary>
    /// Used to determine timeLeft or active state of the projectile.
    /// <para>Defaults to keeping timeLeft above 0 if ability is in use.</para>
    /// </summary>
    public virtual void CheckAbilityActive() {
      if (ability.InUse) {
        projectile.timeLeft = 2;
      }
    }

    /// <summary>
    /// How the projectile behaves.
    /// <para>Defaults to setting projectile center to player center.</para>
    /// </summary>
    public virtual void Behavior() {
      projectile.Center = oPlayer.player.Center;
    }
  }
}
