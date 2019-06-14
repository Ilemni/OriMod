using Terraria.ModLoader;

namespace OriMod.Commands {
  public class OriLight : ModCommand {
    public override string Command => "light";

    public override string Usage => "/light";
    public override string Description => "Toggles player light";

    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      Config.DoPlayerLight = !Config.DoPlayerLight;
    }
  }
}