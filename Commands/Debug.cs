using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  /// <summary>
  /// Enables debug mode, which is basically just more verbose logging and some visual things.
  /// </summary>
  public sealed class DebugCommand : ModCommand {
    public override string Command => "debug";
    public override string Usage => "/debug";
    public override string Description => "Enables debugging the mod. Sorry, there's not much debug mode can do.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      oPlayer.debugMode ^= true;
      Main.NewText("Toggled debug mode to " + oPlayer.debugMode);
    }
  }
}
