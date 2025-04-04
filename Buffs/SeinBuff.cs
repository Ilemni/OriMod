using JetBrains.Annotations;
using OriMod.Projectiles.Minions;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Buffs;

/// <summary>
/// Buff that would keep the <see cref="Sein"/> minion active.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class SeinBuff<TMinion> : ModBuff where TMinion : Sein {
  // ReSharper disable once StaticMemberInGenericType
  private static int _minionType;

  public override string Texture => "OriMod/Buffs/SeinBuff";

  public override void SetStaticDefaults() {
    Main.buffNoSave[Type] = true;
    Main.buffNoTimeDisplay[Type] = true;
    _minionType = ModContent.ProjectileType<TMinion>();
  }

  public override void Update(Player player, ref int buffIndex) {
    player.buffTime[buffIndex] = 18000;
    if (player.ownedProjectileCounts[_minionType] <= 0) {
      player.DelBuff(buffIndex--);
    }
  }
}
