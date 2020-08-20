using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using OriMod.Utilities;

namespace OriMod.NPCs {
  public class OriNPC : GlobalNPC, IBashable {
    public override bool InstancePerEntity => true;
    public OriPlayer BashPlayer { get; set; }
    public Vector2 BashPosition { get; set; }
    public int FramesSinceLastBash { get; private set; }
    public bool IsBashed { get; set; }

    public override void SetDefaults(NPC npc) {
      if (npc.boss) {
        OriUtils.isAnyBossAlive = true;
      }
    }

    public override bool PreAI(NPC npc) {
      if (IsBashed) {
        FramesSinceLastBash = 0;
        npc.Center = BashPosition;
        return false;
      }
      FramesSinceLastBash++;
      return true;
    }

    public override bool SpecialNPCLoot(NPC npc) {
      if (!npc.boss) {
        return false;
      }
      npc.NPCLoot();
      OriUtils.IsAnyBossAlive(check: true);
      OriWorld.ValidateSeinUpgrade();
      return true;
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
      if (IsBashed && target.whoAmI == BashPlayer.player.whoAmI) {
        return false;
      }
      return base.CanHitPlayer(npc, target, ref cooldownSlot);
    }
  }
}
