using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles;

/// <summary>
/// <see cref="GlobalProjectile"/> for handling <see cref="Bash"/>
/// </summary>
[UsedImplicitly]
public sealed class OriProjectile : GlobalProjectile, IBashable {
  private static bool[] _immuneTypes = null!; // SetStaticDefaults()


  public int ImmuneTime => 10;

  public OriPlayer? BashPlayer { get; set; }

  public Vector2 BashPosition { get; set; }

  public bool IsBashed { get; set; }

  public int FramesSinceLastBash { get; private set; }


  public override bool InstancePerEntity => true;


  public override void SetStaticDefaults() {
    _immuneTypes = new bool[ProjectileLoader.ProjectileCount];
    _immuneTypes.AssignValueToKeys(true, stackalloc short[] {
      ProjectileID.FlamethrowerTrap, ProjectileID.FlamesTrap, ProjectileID.GeyserTrap, ProjectileID.SpearTrap,
      ProjectileID.GemHookAmethyst, ProjectileID.GemHookDiamond, ProjectileID.GemHookEmerald,
      ProjectileID.GemHookRuby, ProjectileID.GemHookSapphire, ProjectileID.GemHookTopaz,
      ProjectileID.Hook, ProjectileID.AntiGravityHook, ProjectileID.BatHook, ProjectileID.CandyCaneHook,
      ProjectileID.DualHookBlue, ProjectileID.DualHookRed, ProjectileID.FishHook, ProjectileID.IlluminantHook,
      ProjectileID.LunarHookNebula, ProjectileID.LunarHookSolar, ProjectileID.LunarHookStardust,
      ProjectileID.LunarHookVortex,
      ProjectileID.SlimeHook, ProjectileID.StaticHook, ProjectileID.TendonHook, ProjectileID.ThornHook,
      ProjectileID.TrackHook,
      ProjectileID.WoodHook, ProjectileID.WormHook
    });
  }

  public override void Unload() {
    _immuneTypes = null!;
  }

  /// <summary>
  /// Filter to determine if this <see cref="Projectile"/> can be bashed. Returns true if the projectile should be bashed.
  /// <para>Excludes non-hostile, 0 damage projectiles, minions, sentries, traps, grapples, and projectiles that are already being Bashed.</para>
  /// </summary>
  /// <param name="proj">Projectile to check for bashing.</param>
  /// <returns><see langword="true"/> if the projectile should be bashed, otherwise <see langword="false"/>.</returns>
  public bool CanBeBashed(Projectile proj) {
    return ((IBashable)this).CanBeBashed() && proj is { hostile: true, minion: false, sentry: false, trap: false } &&
      proj.damage != 0 && !_immuneTypes[proj.type];
  }

  public override bool PreAI(Projectile proj) {
    if (IsBashed) {
      FramesSinceLastBash = 0;
      proj.Center = BashPosition;
      proj.friendly = true;
      return false;
    }

    FramesSinceLastBash++;
    return true;
  }

  public override bool ShouldUpdatePosition(Projectile projectile) => !IsBashed;

  public override bool CanHitPlayer(Projectile projectile, Player target) {
    return !IsBashed || (FramesSinceLastBash >= ImmuneTime || BashPlayer is null ||
      target.whoAmI != BashPlayer.Player.whoAmI);
  }
}
