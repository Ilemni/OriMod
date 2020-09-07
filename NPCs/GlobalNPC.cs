using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.NPCs {
  /// <summary>
  /// <see cref="GlobalNPC"/> for handling <see cref="Abilities.Bash"/> and <see cref="OriWorld.GlobalUpgrade"/>
  /// </summary>
  public class OriNPC : GlobalNPC, IBashable {
    public override bool InstancePerEntity => true;
    public OriPlayer BashPlayer { get; set; }
    public Vector2 BashPosition { get; set; }
    public int FramesSinceLastBash { get; private set; }
    public bool IsBashed { get; set; }

    public override bool PreAI(NPC npc) {
      if (IsBashed) {
        FramesSinceLastBash = 0;
        npc.Center = BashPosition;
        return false;
      }
      FramesSinceLastBash++;
      return true;
    }

    public override void NPCLoot(NPC npc) {
      OriWorld.ValidateGlobalUpgrade();
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
      if (IsBashed || FramesSinceLastBash < 15 && !(BashPlayer is null) && target.whoAmI == BashPlayer.player.whoAmI) {
        return false;
      }
      return true;
    }
  }
}
