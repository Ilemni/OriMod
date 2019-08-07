using Terraria;
using Terraria.ModLoader;

namespace OriMod.Buffs {
  public abstract class SeinBuff : ModBuff {
    public override void SetDefaults() {
      Main.buffNoSave[Type] = true;
      Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
      player.buffTime[buffIndex] = 18000;
    }
    
    public override bool Autoload(ref string name, ref string texture) {
      texture = "OriMod/Buffs/SeinBuff";
      return base.Autoload(ref name, ref texture);
    }
  }
}