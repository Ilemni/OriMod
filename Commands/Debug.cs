using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  public class DebugCommand : ModCommand
  {
    public override string Command => "debug";

    public override string Usage => "/oricolor";
    public override string Description => "Enables debugging the mod. Sorry, there's not much debug mode can do.";

    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      oPlayer.debugMode = !oPlayer.debugMode;
    }
  }
}