using AnimLib;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Systems;

/// <summary>
/// System to cancel burrow when teleporting.
/// </summary>
[UsedImplicitly]
public sealed class BurrowCancelOnTeleport : ModSystem {
  public override void Load() {
    OriMod.Debug("Adding hook to Player.Teleport, for cancelling burrow when teleporting.");
    On_Player.Teleport += (orig, self, newPos, style, extraInfo) => {
      Vector2 oldPos = self.position;
      orig(self, newPos, style, extraInfo);
      if (oldPos != newPos && self.GetState<Burrow>() is { Active: true } burrow) {
        burrow.CancelState();
      }
    };
  }
}
