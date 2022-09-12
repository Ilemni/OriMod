using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Abilities {
  /// <summary>
  /// Base class for ability projectiles. These act more as hitboxes and have no visible texture.
  /// </summary>
  public abstract class AbilityProjectile : ModProjectile {
    /// <summary>
    /// Correlates to a <see cref="Id"/>.
    /// </summary>
    public abstract byte Id { get; }

    /// <summary>
    /// The level of the <see cref="OriMod.Abilities.Ability"/> when this <see cref="AbilityProjectile"/> was created.
    /// </summary>
    public int Level => (int)Projectile.ai[0];

    /// <summary>
    /// THe <see cref="OriPlayer"/> that this <see cref="AbilityProjectile"/> belongs to.
    /// </summary>
    public OriPlayer oPlayer => _oPlayer ?? (_oPlayer = Main.player[Projectile.owner].GetModPlayer<OriPlayer>());
    private OriPlayer _oPlayer;

    /// <summary>
    /// The <see cref="OriMod.Abilities.Ability"/> that this <see cref="AbilityProjectile"/> belongs to.
    /// </summary>
    public Ability ability => oPlayer.abilities[Id];

    public override string Texture => "OriMod/Projectiles/Abilities/Blank";

    /// <summary>
    /// <para>Ability projectile SetDefaults(): magic, low timeleft, magic, no tile collision, friendly.</para>
    /// <inheritdoc/>
    /// </summary>
    public override void SetDefaults() {
      Projectile.timeLeft = 2;
      Projectile.penetrate = int.MaxValue;
      Projectile.DamageType = DamageClass.Magic;
      Projectile.tileCollide = false;
      Projectile.ignoreWater = true;
      Projectile.friendly = true;
    }

    public override bool ShouldUpdatePosition() => false;
    public sealed override void AI() {
      CheckAbilityActive();
      Behavior();
    }

    /// <summary>
    /// Used to determine timeLeft or active state of the projectile.
    /// <para>Defaults to keeping timeLeft above 0 if ability is in use.</para>
    /// </summary>
    protected virtual void CheckAbilityActive() {
      if (ability.InUse) {
        Projectile.timeLeft = 2;
      }
    }

    /// <summary>
    /// How the projectile behaves.
    /// <para>Defaults to setting projectile center to player center.</para>
    /// </summary>
    protected virtual void Behavior() {
      Projectile.Center = oPlayer.Player.Center;
    }
  }
}
