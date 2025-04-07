using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Commands;

[UsedImplicitly]
public sealed class AbilityUIDemoCommand : ModCommand {
  public override string Command => "OriAbilityDemo";
  public override CommandType Type => CommandType.Chat;

  private const string EnabledText = "[Ori Ability Demo] Demo enabled.";
  private const string DisabledText = "[Ori Ability Demo] Demo disabled.";

  public override bool IsLoadingEnabled(Mod mod) => Menus.AbilityMenu.LoadingEnabled;

  public override void Action(CommandCaller caller, string input, string[] args) {
    if (Main.netMode != NetmodeID.SinglePlayer) {
      caller.Reply("[Ori Ability Demo] This command can only be used in Single Player.");
      return;
    }

    AbilityUIDemoPlayer demoPlayer = caller.Player.GetModPlayer<AbilityUIDemoPlayer>();
    demoPlayer.ToggleDemo();

    caller.Reply(demoPlayer.IsDemoing ? EnabledText : DisabledText);
  }
}
