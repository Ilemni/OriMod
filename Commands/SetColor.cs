using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  public class SetColor : ModCommand
  {
    public override string Command
      => "oricolor";

    public override string Usage
      => "/oricolor <r> <g> <b> [a]" +
         "\nSets the color of your Ori sprite. Accepted values between 0 and 255";
    public override string Description => "Set the color of your Ori sprite";

    public override CommandType Type => CommandType.Chat;


    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      if (args.Length == 0) {
        Main.NewText("Usage: " + Usage);
      }
      if (args.Length == 1) {
        string lower = args[0].ToLower();
        if (lower == "clear" || lower == "reset") {
          oPlayer.SpriteColor = Color.LightCyan;
          return;
        }
      }
      if (args.Length != 3) {
        throw new UsageException("Expected 3 arguments, got " + args.Length);
      }
      int r = 255;
      int g = 255;
      int b = 255;
      int a = 255;
      if (!int.TryParse(args[0], out r)) {
        throw new UsageException("Expected a number, got " + args[0]);
      }
      if (!int.TryParse(args[1], out g)) {
        throw new UsageException("Expected a number, got " + args[1]);
      }
      if (!int.TryParse(args[2], out b)) {
        throw new UsageException("Expected a number, got " + args[2]);
      }
      oPlayer.SpriteColor = new Color(r, g, b, a);
    }
  }
}