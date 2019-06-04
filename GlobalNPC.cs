using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod
{
  public class BashNPC : GlobalNPC {
    public override bool InstancePerEntity => true;
    public bool IsBashed;
    public Vector2 BashPos;
    public OriPlayer BashPlayer;
    public override bool PreAI(NPC npc) {
      if (IsBashed) {
        npc.Center = BashPos;
        return false;
      }
      return base.PreAI(npc);
    }
  }
}
