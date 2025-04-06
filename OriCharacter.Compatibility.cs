using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace OriMod;

public sealed partial class OriCharacter {
  private static class StarlightCompat {
    private const string ModName = "StarlightRiver";
    private const string PlatformTypeName = "StarlightRiver.Content.NPCs.BaseTypes.MovingPlatform";

    [MemberNotNullWhen(true, nameof(PlatformType))]
    internal static bool StarlightLoaded { get; private set; }

    internal static Type? PlatformType { get; private set; }

    internal static void Initialize() {
      StarlightLoaded = ModLoader.TryGetMod(ModName, out Mod mod);
      if (StarlightLoaded) {
        PlatformType = AssemblyManager.GetLoadableTypes(mod.Code).First(type => type.FullName == PlatformTypeName);
      }
    }
  }

  internal static bool CheckGrounded_StarlightRiverBasePlatform(Player player) {
    if (!StarlightCompat.StarlightLoaded) {
      return false;
    }

    int velY = (int)Math.Max(player.velocity.Y, 0);
    Vector2 playerPos = player.position;
    Rectangle playerRect = new((int)playerPos.X, (int)playerPos.Y + player.height, player.width, 1);
    foreach (NPC npc in Main.ActiveNPCs) {
      if (!StarlightCompat.PlatformType.IsInstanceOfType(npc.ModNPC)) {
        continue;
      }

      Rectangle npcRect = npc.Hitbox;
      npcRect.Height = 8 + velY + (int)Math.Abs(npc.velocity.Y);

      if (playerRect.Intersects(npcRect) && playerPos.Y <= npc.position.Y) {
        return true;
      }
    }

    return false;
  }
}
