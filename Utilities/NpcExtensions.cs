using Terraria;

namespace OriMod.Utilities;

public static class NpcExtensions {
  public static NPC HeadOrSelf(this NPC self) => self.realLife == -1 ? self : Main.npc[self.realLife];
}
