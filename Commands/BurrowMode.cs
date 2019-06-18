using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public class BurrowMode : ModCommand {
    public override string Command => "burrowmode";
    public override string Usage => "/burrowmode";
    public override string Description => "Changes if Burrow is moved by movement keys or following mouse.";
    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) {
      Config.BurrowToMouse = !Config.BurrowToMouse;
      Main.NewText("Toggled burrow mode to " + (Config.BurrowToMouse ? "mouse" : "arrow keys") + ".");
    }
  }
}