using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.NPCs;

/// <summary>
/// <see cref="GlobalNPC"/> for handling <see cref="Abilities.Bash"/>
/// </summary>
[UsedImplicitly]
public sealed class OriNpc : GlobalNPC, IBashable {
  [field: AllowNull, MaybeNull]
  private static bool[] ImmuneTypes {
    get => field ??= CreateImmuneTypes();
    set;
  }


  public int ImmuneTime => 15;

  public OriPlayer? BashPlayer { get; set; }

  public bool IsBashed { get; set; }

  public int FramesUntilBashable { get; set; }


  public override bool InstancePerEntity => true;


  private static bool[] CreateImmuneTypes() {
    return new bool[NPCLoader.NPCCount].WithTrueValues([
      NPCID.EaterofWorldsBody, NPCID.EaterofWorldsHead, NPCID.EaterofWorldsTail,
      NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail,
      NPCID.BlazingWheel, NPCID.SpikeBall,
      NPCID.DD2EterniaCrystal, NPCID.DD2LanePortal,
      NPCID.CultistTablet,
      NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerStardust, NPCID.LunarTowerVortex
    ]);
  }

  public override void Unload() {
    ImmuneTypes = null!;
  }

  public override bool AppliesToEntity(NPC npc, bool lateInstantiation) {
    // Relies on npc.friendly, .boss
    return lateInstantiation &&
      npc is { friendly: false, boss: false } &&
      !ImmuneTypes[npc.type];
  }

  public override bool PreAI(NPC npc) {
    if (IsBashed) {
      return false;
    }

    if (FramesUntilBashable > 0) {
      FramesUntilBashable--;
    }

    return true;
  }

  public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
    return !IsBashed && FramesUntilBashable == 0;
  }

  public override void OnKill(NPC npc) {
    ((IBashable)this).ClearBashPlayer();
  }
}
