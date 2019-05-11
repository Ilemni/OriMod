using Terraria;
using Terraria.ModLoader;

namespace OriMod.Buffs {
	public class SeinBuffBase : ModBuff {
		public OriPlayer modPlayer = null;
		public int upgrade;
		public override void SetDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;
		}
	}
}