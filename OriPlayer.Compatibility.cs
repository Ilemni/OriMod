using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod;

public sealed partial class OriPlayer {
  #region Variables

  private static Type? _starlightRiverBasePlatform;

  #endregion

  private static bool CheckGrounded_StarlightRiverBasePlatform(Player player) {
    if (_starlightRiverBasePlatform is null) {
      return false;
    }

    int velY = (int)Math.Max(player.velocity.Y, 0);
    Rectangle playerRect = new((int)player.position.X, (int)player.position.Y + player.height, player.width, 1);
    foreach (NPC npc in Main.ActiveNPCs) {
      if (!_starlightRiverBasePlatform.IsInstanceOfType(npc.ModNPC)) {
        continue;
      }

      Rectangle npcRect = new((int)npc.position.X, (int)npc.position.Y, npc.width,
        8 + velY + (int)Math.Abs(npc.velocity.Y));

      if (playerRect.Intersects(npcRect) && player.position.Y <= npc.position.Y)
        return true;
    }

    return false;
  }
}
