using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
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
  private static bool[] _immuneTypes = null!; // SetStaticDefaults()


  public int ImmuneTime => 15;

  public OriPlayer? BashPlayer { get; set; }

  public Vector2 BashPosition { get; set; }

  public bool IsBashed { get; set; }

  public int FramesSinceLastBash { get; private set; }


  public override bool InstancePerEntity => true;


  public override void SetStaticDefaults() {
    _immuneTypes = new bool[NPCLoader.NPCCount];
    _immuneTypes.AssignValueToKeys(true, stackalloc short[] {
      NPCID.EaterofWorldsBody, NPCID.EaterofWorldsHead, NPCID.EaterofWorldsTail,
      NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail,
      NPCID.BlazingWheel, NPCID.SpikeBall,
      NPCID.DD2EterniaCrystal, NPCID.DD2LanePortal,
      NPCID.CultistTablet,
      NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerStardust, NPCID.LunarTowerVortex
    });
  }

  public override void Unload() {
    _immuneTypes = null!;
  }

  /// <summary>
  /// Filter to determine if this <see cref="NPC"/> can be bashed. Returns <see langword="true"/> if the NPC should be bashed.
  /// <para>Excludes friendly NPCs, bosses, specific NPCs, and NPCs that are already being Bashed.</para>
  /// </summary>
  /// <param name="npc"><see cref="NPC"/> to check.</param>
  /// <returns><see langword="true"/> if the NPC should be bashed, otherwise <see langword="false"/>.</returns>
  public bool CanBeBashed(NPC npc) {
    return ((IBashable)this).CanBeBashed() && npc is { friendly: false, boss: false } &&
      !_immuneTypes[npc.type];
  }

  public override bool PreAI(NPC npc) {
    if (IsBashed) {
      if (!IBashable.ValidateBash(BashPlayer, npc)) {
        IsBashed = false;
        BashPlayer = null;
        return true;
      }

      FramesSinceLastBash = 0;
      npc.Center = BashPosition;
      return false;
    }

    FramesSinceLastBash++;
    return true;
  }

  public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
    return !IsBashed && (FramesSinceLastBash >= ImmuneTime || BashPlayer is null ||
      target.whoAmI != BashPlayer.Player.whoAmI);
  }
}
