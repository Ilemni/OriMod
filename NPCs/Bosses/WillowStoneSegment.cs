using Terraria;
using Terraria.ModLoader;

namespace OriMod.NPCs.Bosses {
  public class WillowStoneSegment : ModNPC {
    public NPC Owner => Main.npc[(int)npc.ai[0]];
    public int ID => (int)npc.ai[1];
  }
}
