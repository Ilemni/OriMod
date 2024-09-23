using JetBrains.Annotations;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles;

/// <summary>
/// <see cref="GlobalProjectile"/> for handling <see cref="global::OriMod.Abilities.Bash"/>
/// </summary>
[UsedImplicitly]
public sealed class OriProjectile : GlobalProjectile, IBashable {
  private static bool[] ImmuneTypes => _immuneTypes ??= CreateImmuneTypes();

  private static bool[]? _immuneTypes;


  public int ImmuneTime => 10;

  public OriPlayer? BashPlayer { get; set; }

  public bool IsBashed { get; set; }

  public int FramesUntilBashable { get; set; }


  public override bool InstancePerEntity => true;


  private static bool[] CreateImmuneTypes() {
    bool[] result = new bool[ProjectileLoader.ProjectileCount];
    result.AssignValueToKeys(true, stackalloc short[] {
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
    return result;
  }

  public override void Unload() {
    _immuneTypes = null!;
  }

  public override bool AppliesToEntity(Projectile proj, bool lateInstantiation) {
    // Relies on projectile.hostile, .minion, .sentry, .trap
    return lateInstantiation &&
      proj is { hostile: true, minion: false, sentry: false, trap: false } &&
      proj.damage != 0 && !ImmuneTypes[proj.type];
  }

  public override bool PreAI(Projectile proj) {
    if (IsBashed) {
      proj.friendly = true;
      return false;
    }

    if (FramesUntilBashable > 0) {
      FramesUntilBashable--;
    }

    return true;
  }

  public override bool ShouldUpdatePosition(Projectile projectile) => !IsBashed;

  public override bool CanHitPlayer(Projectile projectile, Player target) {
    return !IsBashed && FramesUntilBashable == 0;
  }
}
