using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles {
  public class OriProjectile : GlobalProjectile, IBashable {
    public override bool InstancePerEntity => true;
    public OriPlayer BashPlayer { get; set; }
    public Entity BashEntity { get; set; }
    public bool IsBashed { get; set; }
    public Vector2 BashPosition { get; set; }

    public void OnBashed(OriPlayer oPlayer) {

    }

    public override bool PreAI(Projectile proj) {
      if (IsBashed) {
        proj.Center = BashPosition;
        proj.friendly = true;
        return false;
      }
      return true;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target) => projectile.CanHit(target) && (!IsBashed || target != BashPlayer.player);
  }
}
