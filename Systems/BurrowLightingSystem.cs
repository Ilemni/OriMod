using AnimLib;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace OriMod.Systems;

/// <summary>
/// When the player is using <see cref="Burrow"/>,
/// forces lighting updates to occur every frame.
/// </summary>
[UsedImplicitly]
public sealed class BurrowLightingSystem : ModSystem {
  public override void Load() {
    OriMod.Debug("Adding hook to LightingEngine.ProcessArea, " +
      "for forcing lighting updates when burrowing. (Optional, experimental config enables it)");
    On_LightingEngine.ProcessArea += ProcessArea;
  }

  private static void ProcessArea(On_LightingEngine.orig_ProcessArea orig, LightingEngine self, Rectangle area) {
    orig(self, area);

    if (BurrowForceLighting()) {
      while (Main.renderCount != 3) {
        orig(self, area);
      }
    }
  }

  private static bool BurrowForceLighting() {
    return OriMod.ConfigClient.eBurrowForceLighting &&
      Main.LocalPlayer is { active: true } player &&
      player.GetState<Burrow>() is { ActiveTime: > 4 };
  }
}
