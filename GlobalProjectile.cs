using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public class OriProjectile : GlobalProjectile {
    public override bool InstancePerEntity => true;
    public bool IsBashed;
    public Vector2 BashPos;
    public OriPlayer BashPlayer;
    public override bool PreAI(Projectile proj) {
      if (IsBashed) {
        proj.Center = BashPos;
        proj.friendly = true;
        return false;
      }
      return base.PreAI(proj);
    }
    public override bool CanHitPlayer(Projectile projectile, Player target) => IsBashed && target == BashPlayer.player ? false : base.CanHitPlayer(projectile, target);
  }
}