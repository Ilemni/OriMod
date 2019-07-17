using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  public class SetColor : ModCommand {
    public override CommandType Type => CommandType.Chat;

    public override string Command => "color";
    public override string Description => "Set the color of your Spirit Guardian.";
    public override string Usage
      => "/color <r> <g> <b>\n/color <clear|reset>\n" +
         "Values between 0 and 255";

    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      if (args.Length == 0) {
        Main.NewTextMultiline("Usage: " + Usage);
        return;
      }
      if (args.Length == 1) {
        string lower = args[0].ToLower();
        if (lower == "clear" || lower == "reset") {
          OriMod.ConfigClient.PlayerColor = Color.LightCyan;
          return;
        }
      }
      if (args.Length != 3) {
        throw new UsageException($"Expected 3 arguments, got {args.Length}");
      }
      byte r = 255, g = 255, b = 255, a = 255;
      if (!byte.TryParse(args[0], out r)) {
        throw new UsageException($"Expected a number between 0 and 255 for red, got {args[0]}");
      }
      if (!byte.TryParse(args[1], out g)) {
        throw new UsageException($"Expected a number between 0 and 255 for green, got {args[1]}");
      }
      if (!byte.TryParse(args[2], out b)) {
        throw new UsageException($"Expected a number between 0 and 255 for blue, got {args[2]}");
      }
      OriMod.ConfigClient.PlayerColor = new Color(r, g, b, a);
      Main.NewText("Set Spirit Guardian color to " + OriMod.ConfigClient.PlayerColor);
    }
  }
}