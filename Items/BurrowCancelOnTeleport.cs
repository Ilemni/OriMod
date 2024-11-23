using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Items;

public class BurrowCancelOnTeleport : ModSystem {
  public override void Load() => On_Player.Teleport += On_Player_Teleport;

  public override void Unload() => On_Player.Teleport -= On_Player_Teleport;

  private static void On_Player_Teleport(On_Player.orig_Teleport orig, Player self, Vector2 newPos, int style, int extraInfo) {
    Vector2 oldPos = self.position;
    orig(self, newPos, style, extraInfo);
    if (oldPos != newPos && self.GetModPlayer<OriPlayer>().ActiveState is Burrow burrow) {
      burrow.CancelState();
    }
  }
}
