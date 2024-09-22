using AnimLib.States;
using AnimLib.Projectiles;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

public abstract class OriAbility(Player player) : AbilityState(player) {
  // ReSharper disable once InconsistentNaming
  protected Player Player => Entity;
  protected OriPlayer OriPlayer { get; private set; } = null!; // OnInitialize()
  protected OriInput Input { get; private set; } = null!; // OnInitialize()

  protected bool OnWall => OriPlayer.OnWall;
  protected bool IsGrounded => OriPlayer.IsGrounded;

  protected override void OnInitialize() {
    OriPlayer = Player.GetModPlayer<OriPlayer>();
    Input = OriPlayer.Input;
  }

  public override bool CanEnter() =>
    base.CanEnter() && !Player.mount.Active &&
    Player is { dead: false, frozen: false, stoned: false, webbed: false, shimmering: false };

  protected void RestoreAirJumps() => OriPlayer.RestoreAirJumps();

  internal void RefreshParticles(Color col) {
    // TODO: Replace with better indicator of ability being off cooldown
    int dustType = ModContent.DustType<AbilityRefreshedDust>();
    for (int i = 0; i < 10; i++) {
      Dust.NewDust(Player.Center, 12, 12, dustType, newColor: col);
    }
  }

  /// <summary>
  /// Creates a new <see cref="AbilityProjectile{T}"/> of type <typeparamref name="T"/>.
  /// Assigns <see cref="AbilityProjectile{T}.Ability"/> to this ability, and
  /// <see cref="AbilityProjectile{T}.Level"/> to this ability's level.
  /// </summary>
  /// <param name="offset">Positional offset from the player's center.</param>
  /// <param name="velocity">Starting speed of the projectile.</param>
  /// <param name="damage">Damage value of the projectile.</param>
  /// <param name="knockBack">Knockback strength of the projectile.</param>
  /// <typeparam name="T">Type of ability projectile.</typeparam>
  /// <returns>A new <see cref="AbilityProjectile{T}"/> of type <typeparamref name="T"/>.</returns>
  protected T NewAbilityProjectile<T>(Vector2 offset = default, Vector2 velocity = default,
    int damage = 0,
    float knockBack = 0)
    where T : AbilityProjectile {
    Projectile projectile = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center + offset,
      velocity, ModContent.ProjectileType<T>(), damage, knockBack, Player.whoAmI);

    T modProjectile = (T)projectile.ModProjectile;
    modProjectile.Ability = this;
    modProjectile.Level = Level;
    return modProjectile;
  }

  /// <summary>
  /// Shorthand for <see cref="State.TriggerState{T}">
  /// TriggerStateFromAny</see> to <see cref="NoAbility"/>
  /// </summary>
  public void CancelState() => TriggerState<NoAbility>();
}
