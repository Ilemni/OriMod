using Terraria;
using Terraria.ModLoader;

namespace OriMod.Buffs; 

/// <summary>
/// Buff that would keep the <see cref="Projectiles.Minions.Sein"/> minion active.
/// </summary>
public abstract class SeinBuff : ModBuff {
  public override string Texture => @"OriMod/Buffs/SeinBuff";
       
  public override void SetStaticDefaults() {
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
