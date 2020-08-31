using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Commands {
  /// <summary>
  /// Enables debug mode, which is basically just more verbose logging and some visual things.
  /// </summary>
  public sealed class AbilityCommand : ModCommand {
    public override string Command => "ability";
    public override string Usage => "/ability <ability name OR internal ability ID> [level]";
    public override string Description => "Sets the level of the specified ability to the specified value.";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      OriPlayer oPlayer = caller.Player.GetModPlayer<OriPlayer>();
      if (!oPlayer.debugMode) {
        Main.NewText($"This command cannot be used outside of debug mode.", Color.Red);
        return;
      }

      if (args.Length < 1) {
        Main.NewText($"Expected one or two arguments, got {args.Length}.", Color.Red);
        return;
      }

      if (args[0] == "unlockall") {
        oPlayer.abilities.UnlockAllAbilities();
        Main.NewText($"Unlocked all abilities.", Color.LightGreen);
        return;
      }

      Ability ability = null;
      if (int.TryParse(args[0], out int id)) {
        if (id < 0 || id > AbilityID.Count) {
          Main.NewText($"\"{id}\" does not map to a valid AbilityID.", Color.Red);
          return;
        }
        ability = oPlayer.abilities[id];
      }
      else {
        foreach (var ab in oPlayer.abilities) {
          if (ab.GetType().Name.ToLower() == args[0]) {
            ability = ab;
          }
        }
        if (ability == null) {
          Main.NewText($"\"{args[0]}\" is not a valid Ability.", Color.Red);
          return;
        }
      }

      if (ability is ILevelable levelable) {
        if (args.Length < 2) {
          Main.NewText($"{ability.GetType().Name}'s Level is {levelable.Level}.", Color.LightGreen);
        }
        else if (byte.TryParse(args[1], out byte level)) {
          levelable.Level = level;
          Main.NewText($"{ability.GetType().Name}'s Level has been set to {level}.", Color.LightGreen);
        }
        else {
          Main.NewText($"\"{args[1]}\" is not a valid input for level.", Color.Red);
        }
      }
      else {
        Main.NewText($"{ability.GetType().Name} cannot have its level read or modified.", Color.Red);
      }
    }
  }
}
