using Terraria;
using Terraria.ModLoader;

namespace OriMod.Buffs {
  public abstract class SeinBuff : ModBuff {
    public override bool Autoload(ref string name, ref string texture) {
      texture = "OriMod/Buffs/SeinBuff";
      return base.Autoload(ref name, ref texture);
    }

    public override void SetDefaults() {
      Main.buffNoSave[Type] = true;
      Main.buffNoTimeDisplay[Type] = true;
    }

    protected abstract int ProjectileType();

    public override void Update(Player player, ref int buffIndex) {
      if (player.ownedProjectileCounts[ProjectileType()] > 0) {
        player.buffTime[buffIndex] = 18000;
      }
      else {
        player.DelBuff(buffIndex);
        buffIndex--;
      }
    }
  }
}
