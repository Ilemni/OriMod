using Terraria;
using Terraria.ModLoader;

namespace OriMod
{
  public class BashNPC : GlobalNPC {
    public override bool PreAI(NPC npc) { // TODO: Multiplayer sync
      if (npc.boss || npc.immortal || OriPlayer.CannotBash.Contains(npc.type)) return true;
      return !(
        Main.LocalPlayer.GetModPlayer<OriPlayer>().bashActive &&
        Main.LocalPlayer.GetModPlayer<OriPlayer>().bashNPC == npc.whoAmI 
      );
    }
  }
}
