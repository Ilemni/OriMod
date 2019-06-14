using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  public class ConfigCommand : ModCommand {
    public override string Command => "config";
    public override string Usage => "/config <load|save>";
    public override string Description => "Allows loading and saving the config file in-game";
    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) {
      if (args.Length == 0) {
        Main.NewText("Usage: " + Usage);
        return;
      }
      switch (args[0].ToLower()) {
        case "load":
          bool success = Config.ReadConfig();
          Main.NewText(success ? "Loaded config." : "Error reading config.");
          break;
        case "save":
          Config.CreateConfig();
          Main.NewText("Saved config.");
          break;
        default:
          Main.NewText("Expected an argument of \"load\" or \"save\"");
          return;
      }
      OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
      oPlayer.SpriteColor = Config.OriColor;
    }
  }
}