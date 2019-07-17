using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  public class ConfigCommand : ModCommand {
    public override string Command => "config";
    public override string Usage => "/config <load|save|reset>";
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
          Config.SaveConfig();
          Main.NewText("Saved config.");
          break;
        case "reset":
          Config.ResetConfig();
          Main.NewText("Reset config.");
          break;
        default:
          Main.NewText("Expected an argument of \"load\", \"save\", or \"reset\"");
          return;
      }
    }
  }
} 