using Terraria;
using Terraria.ModLoader;

namespace OriMod
{
  public class BashNPC : GlobalNPC {
    public override bool PreAI(NPC npc) { // TODO: Multiplayer sync
      if (npc.boss || npc.immortal || Abilities.Bash.CannotBash.Contains(npc.type)) return true;
      OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
      return !(oPlayer.Abilities.bash.InUse && oPlayer.Abilities.bash.BashNpcID == npc.whoAmI);
    }
  }
}
