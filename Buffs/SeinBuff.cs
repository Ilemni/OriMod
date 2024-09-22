using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Buffs;

/// <summary>
/// Buff that would keep the <see cref="Projectiles.Minions.Sein"/> minion active.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class SeinBuff<TMinion> : ModBuff where TMinion : Projectiles.Minions.Sein {
  public override string Texture => "OriMod/Buffs/SeinBuff";

  public override void SetStaticDefaults() {
    Main.buffNoSave[Type] = true;
    Main.buffNoTimeDisplay[Type] = true;
    _minionType = ModContent.ProjectileType<TMinion>();
  }

  // ReSharper disable once StaticMemberInGenericType
  private static int _minionType;

  public override void Update(Player player, ref int buffIndex) {
    if (player.ownedProjectileCounts[_minionType] > 0) {
      player.buffTime[buffIndex] = 18000;
    }
    else {
      player.DelBuff(buffIndex);
      buffIndex--;
    }
  }
}
