using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles {
  /// <summary>
  /// <see cref="GlobalProjectile"/> for handling <see cref="Bash"/>
  /// </summary>
  public class OriProjectile : GlobalProjectile, IBashable {
    public override bool InstancePerEntity => true;
    public OriPlayer BashPlayer { get; set; }
    public Vector2 BashPosition { get; set; }
    public int FramesSinceLastBash { get; private set; }
    public bool IsBashed { get; set; }

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

    public override bool CanHitPlayer(Projectile projectile, Player target) {
      return !IsBashed || target != BashPlayer.player;
    }
  }
}