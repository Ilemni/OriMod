using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  public abstract class Minion : ModProjectile {
    public override sealed void AI() {
      CheckActive();
      Behavior();
    }

    internal abstract void CheckActive();

    internal abstract void Behavior();
  }
}
