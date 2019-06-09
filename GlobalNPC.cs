using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace OriMod
{
  public class OriNPC : GlobalNPC {
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
    private void GetDownState(NPC npc, ref bool downedBoss, out bool wasDowned, out bool nowDowned) {
      wasDowned = downedBoss;
      npc.NPCLoot();
      nowDowned = downedBoss;
    }
    private bool DoUpdate(NPC npc, ref bool downedBoss) {
      bool wasDowned = false;
      bool nowDowned = false;
      GetDownState(npc, ref downedBoss, out wasDowned, out nowDowned);
      return (nowDowned && !wasDowned);
    }
    private void GetDownCount(NPC npc, out int oldCount, out int newCount, ref bool downedBoss1, ref bool downedBoss2, ref bool downedBoss3) {
      oldCount = 0;
      newCount = 0;
      if (downedBoss1) oldCount++;
      if (downedBoss2) oldCount++;
      if (downedBoss3) oldCount++;
      npc.NPCLoot();
      if (downedBoss1) newCount++;
      if (downedBoss2) newCount++;
      if (downedBoss3) newCount++;
    }
    private void GetDownCount(NPC npc, out int oldCount, out int newCount, ref bool downedBoss1, ref bool downedBoss2, ref bool downedBoss3, ref bool downedBoss4) {
      oldCount = 0;
      newCount = 0;
      if (downedBoss1) oldCount++;
      if (downedBoss2) oldCount++;
      if (downedBoss3) oldCount++;
      if (downedBoss4) oldCount++;
      npc.NPCLoot();
      if (downedBoss1) newCount++;
      if (downedBoss2) newCount++;
      if (downedBoss3) newCount++;
      if (downedBoss4) newCount++;
    }
    public override bool SpecialNPCLoot(NPC npc) {
      if (!npc.boss) {
        return base.SpecialNPCLoot(npc);
      }
      bool doUpdate = false;
      byte upgrade = 0;
      int oldCount = 0;
      int newCount = 0;
      switch (npc.type) {
        case NPCID.EyeofCthulhu:
          upgrade = 1;
          doUpdate = DoUpdate(npc, ref NPC.downedBoss1);
          break;
        case NPCID.EaterofWorldsHead:
        case NPCID.EaterofWorldsBody:
        case NPCID.EaterofWorldsTail:
        case NPCID.BrainofCthulhu:
          upgrade = 2;
          doUpdate = DoUpdate(npc, ref NPC.downedBoss2);
          break;
        case NPCID.SkeletronHead:
          upgrade = 3;
          doUpdate = DoUpdate(npc, ref NPC.downedBoss3);
          break;
        case NPCID.WallofFlesh:
          upgrade = 4;
          doUpdate = DoUpdate(npc, ref Main.hardMode);
          break;
        case NPCID.SkeletronPrime:
        case NPCID.Spazmatism:
        case NPCID.Retinazer:
        case NPCID.TheDestroyer:
          GetDownCount(npc, out oldCount, out newCount, ref NPC.downedMechBoss1, ref NPC.downedMechBoss2, ref NPC.downedMechBoss3);
          if (newCount == 1 && oldCount != 1) {
            upgrade = 5;
            doUpdate = true;
          }
          else if (newCount == 3 && oldCount != 3) {
            upgrade = 6;
            doUpdate = true;
          }
          break;
        case NPCID.Plantera:
          upgrade = 7;
          DoUpdate(npc, ref NPC.downedPlantBoss);
          break;
        case NPCID.GolemHead:
          upgrade = 8;
          doUpdate = DoUpdate(npc, ref NPC.downedGolemBoss);
          break;
        case NPCID.CultistBoss:
          upgrade = 9;
          doUpdate = DoUpdate(npc, ref NPC.downedAncientCultist);
          break;
        case NPCID.LunarTowerNebula:
        case NPCID.LunarTowerSolar:
        case NPCID.LunarTowerStardust:
        case NPCID.LunarTowerVortex:
          GetDownCount(npc, out oldCount, out newCount, ref NPC.downedTowerNebula, ref NPC.downedTowerSolar, ref NPC.downedTowerStardust, ref NPC.downedTowerStardust);
          if (newCount == 2 && oldCount != 2) {
            upgrade = 10;
            doUpdate = true;
          }
          else if (newCount == 4 && oldCount != 4) {
            upgrade = 11;
            doUpdate = true;
          }
          break;
        case NPCID.MoonLordCore:
          upgrade = 12;
          doUpdate = DoUpdate(npc, ref NPC.downedMoonlord);
          break;
        default:
          npc.NPCLoot();
          break;
      }
      if (doUpdate) OriWorld.UpdateOriPlayerSeinStates(upgrade);
      return true;
    }
  }
}
