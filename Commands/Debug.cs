using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  /// <summary>
  /// Enables debug mode, which is basically just more verbose logging and some visual things.
  /// </summary>
  public sealed class DebugCommand : ModCommand {
    public override string Command => "oridebug";
    public override string Usage => "/oridebug";
    public override string Description => "Enables debug testing of the mod. Allows testing abilities of various levels.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      oPlayer.debugMode ^= true;
      Main.NewText("Toggled debug mode to " + oPlayer.debugMode);
    }
  }
}
