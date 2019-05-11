using Terraria;
using Terraria.ModLoader;

namespace OriMod
{
  public class BashNPC : GlobalNPC {
    public override bool PreAI(NPC npc) {
      return !(
        Main.LocalPlayer.GetModPlayer<OriPlayer>().bashNPC == npc.whoAmI && 
        Main.LocalPlayer.GetModPlayer<OriPlayer>().bashActive
      );
    }
  }
}
